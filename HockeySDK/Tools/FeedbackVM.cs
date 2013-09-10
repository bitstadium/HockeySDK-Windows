using HockeyApp.Model;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Reactive;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HockeyApp.Tools
{
    public class FeedbackVM : INotifyPropertyChanged
    {
        string feedbackThreadToken;
        string appIdentifier;
        public Task Initialization { get; private set; }

        public FeedbackVM(string appIdentifier)
        {
            this.appIdentifier = appIdentifier;
            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
            if (settings.Contains(Constants.FeedbackThreadKey))
            {
                feedbackThreadToken = settings[Constants.FeedbackThreadKey] as string;
            }
            else
            {
                //TODO testing rausmachen
                //feedbackThreadToken = "cbb777763cccf0e3b3ee631e45180067";
            }

            Initialization = InitializeAsync(); //await this.Initialization to make shure its inititalized
        }

        private async Task InitializeAsync()
        {
            if (this.IsThreadAvailable)
            {
                //TODO errorhandling
                var thread = await this.FetchFeedbackThreadAsync();
                this.ThreadInfo = thread.messages.Count + " Messages";
                foreach (var msg in ((IEnumerable<FeedbackMessage>)thread.messages).Reverse())
                {
                    this.Messages.Add(msg);
                }
                this.IsMessageListVisible = true;
            }
        }

        #region Properties

        private string threadinfo;
        public string ThreadInfo
        {
            get { return threadinfo; }
            set { threadinfo = value;
            NotifyOfPropertyChange("ThreadInfo");
            }
        }

        private Boolean isMessageListVisible = false;
        public Boolean IsMessageListVisible
        {
            get { return isMessageListVisible; }
            set
            {
                isMessageListVisible = value;
                NotifyOfPropertyChange("IsMessageListVisible");
                NotifyOfPropertyChange("IsMessageFormVisible");
            }
        }

        public Boolean IsMessageFormVisible
        {
            get { return !IsMessageListVisible; }
        }


        private string email;
        public string Email
        {
            get { return email; }
            set { email = value;
            NotifyOfPropertyChange("Email");
            }
        }

        private string subject;
        public string Subject
        {
            get { return subject; }
            set { subject = value;
            NotifyOfPropertyChange("Subject");
            }
        }

        private string message;
        public string Message
        {
            get { return message; }
            set { message = value;
            NotifyOfPropertyChange("Message");
            }
        }

        private string username;
        public string Username
        {
            get { return username; }
            set { username = value;
            NotifyOfPropertyChange("Username");
            }
        }

        #endregion

        ObservableCollection<FeedbackMessage> messages = new ObservableCollection<FeedbackMessage>();
        public ObservableCollection<FeedbackMessage> Messages
        {
            get { return messages; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsThreadAvailable
        {
            get { return feedbackThreadToken != null; }
        }

        public async Task<bool> SubmitForm() {
            if(!this.Email.IsValidEmail() || this.Subject.IsEmpty() || this.Username.IsEmpty()) {
                MessageBox.Show("Fill required fields!"); //TODO i18n
                return false;
            }

            var msg = new FeedbackMessage()
            {
                text = this.Message,
                email = this.Email,
                subject = this.Subject,
                name = this.Username
            };

            if (IsThreadAvailable)
            {
                var fbResponse = await this.PostAnswerAsync(msg);
                if (fbResponse != null)
                {
                    this.Messages.Add(fbResponse.feedback.messages.Last());
                    await this.ShowList();
                }
            }
            else
            {
                var fbResponse = await this.StartThreadAsync(msg);
                if (fbResponse != null)
                {
                    this.feedbackThreadToken = fbResponse.token;
                    this.PersistThreadMetaInfos(fbResponse);
                    this.Messages.Add(fbResponse.feedback.messages.Last());
                    await this.ShowList();
                }
            }
            return true;
        }

        public void PersistThreadMetaInfos(FeedbackResponseSingle fbResponse)
        {
            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
            settings.PutInfo(Constants.FeedbackThreadKey, fbResponse.token);
            settings.PutInfo(Constants.FeedbackEmailKey, this.Email);
            settings.PutInfo(Constants.FeedbackUsernameKey, this.Username);
            settings.Save();
        }

        public async Task ShowForm()
        {
            
        }

        public async Task ShowList()
        {
            this.ResetForm(); //ja oder nein oder eigener Button
            Scheduler.Dispatcher.Schedule(() => { this.IsMessageListVisible = false; });
        }

        private void ResetForm()
        {
            this.Message = "";
        }

        public void Reload()
        {
        }

        public async Task<FeedbackThread> FetchFeedbackThreadAsync()
        {
            var request = WebRequest.CreateHttp(new Uri(Constants.ApiBase + "apps/" + this.appIdentifier + "/feedback/" + this.feedbackThreadToken + ".json", UriKind.Absolute));
            request.Method = HttpMethod.Get;
            request.Headers[HttpRequestHeader.UserAgent] = Constants.UserAgentString;

            if (NetworkInterface.GetIsNetworkAvailable())
            {
                try
                {
                    var response = await request.GetResponseTaskAsync();
                    var fbResp = await Task.Run<FeedbackResponseSingle>(() => FeedbackResponseSingle.FromJson(response.GetResponseStream()));
                    if (fbResp.status.Equals("success"))
                    {
                        return fbResp.feedback;
                    }
                }
                catch (Exception e)
                {
                    throw;
                }
            }
            return null;
        }

        public async Task<FeedbackResponseSingle> StartThreadAsync(FeedbackMessage message)
        {
            var request = WebRequest.CreateHttp(new Uri(Constants.ApiBase + "apps/" + this.appIdentifier + "/feedback", UriKind.Absolute));
            request.Method = HttpMethod.Post;
            request.Headers[HttpRequestHeader.UserAgent] = Constants.UserAgentString;
            await request.SetPostDataAsync(message.SerializeToWwwForm());

            if (NetworkInterface.GetIsNetworkAvailable())
            {
                try
                {
                    var response = await request.GetResponseTaskAsync();
                    var fbResp = await Task.Run<FeedbackResponseSingle>(() => FeedbackResponseSingle.FromJson(response.GetResponseStream()));
                    if (fbResp.status.Equals("success"))
                    {
                        return fbResp;
                    }
                }
                catch (Exception e)
                {
                    throw;
                }
            }
            return null;
        }

        public async Task<FeedbackResponseSingle> PostAnswerAsync(FeedbackMessage message)
        {
            var request = WebRequest.CreateHttp(new Uri(Constants.ApiBase + "apps/" + this.appIdentifier + "/feedback/" + this.feedbackThreadToken + "/", UriKind.Absolute));
            request.Method = HttpMethod.Put;
            request.Headers[HttpRequestHeader.UserAgent] = Constants.UserAgentString;
            await request.SetPostDataAsync(message.SerializeToWwwForm());

            if (NetworkInterface.GetIsNetworkAvailable())
            {
                try
                {
                    var response = await request.GetResponseTaskAsync();
                    var fbResp = await Task.Run<FeedbackResponseSingle>(() => FeedbackResponseSingle.FromJson(response.GetResponseStream()));
                    if (fbResp.status.Equals("success"))
                    {
                        return fbResp;
                    }
                }
                catch (Exception e)
                {
                    throw;
                }
            }
            return null;
        }

        #region INotify
        protected void NotifyOfPropertyChange(string name)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
        #endregion

    }
}
