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
using HockeyApp.Tools;

namespace HockeyApp.ViewModels
{

    public class FeedbackVM : VMBase
    {

        string appIdentifier;
        public Task Initialization { get; private set; }
        Action<bool> showFormAppBarAction;

        public FeedbackVM(Action<bool> showFormAppBarAction)
        {
            this.appIdentifier = FeedbackManager.Instance.AppIdentitfier;
            this.ThreadInfo = FeedbackManager.Instance.FeedbackPageTopTitle;
            this.showFormAppBarAction = showFormAppBarAction;
            Initialization = InitializeAsync(); //await this.Initialization to make shure its inititalized
        }

        private async Task InitializeAsync()
        {
            if (FeedbackManager.Instance.IsThreadOpen)
            {
                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    try
                    {
                        FeedbackThread thread = await FeedbackManager.Instance.GetActiveThreadAsync();
                        if (thread != null)
                        {
                            foreach (var msg in (thread.messages))
                            {
                                this.Messages.Add(new FeedbackMessageVM(msg));
                            }
                            if (FeedbackManager.Instance.FeedbackPageTopTitle.IsEmpty() && !thread.messages.First().subject.IsEmpty())
                            {
                                Deployment.Current.Dispatcher.BeginInvoke(() => this.ThreadInfo = thread.messages.First().subject);
                            }
                        }
                        else //thread has been deleted
                        {
                            Deployment.Current.Dispatcher.BeginInvoke(() => IsThreadActive = false);
                            SwitchToMessageForm();
                        }
                    }
                    catch (Exception e)
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            MessageBox.Show("There has been a connection problem with the server. Please try again later.");
                        });
                    }
                }
                else //no internet connection
                {
                    //TODO warnung kein internet und zurückspringen (navigateback!?)
                }
            }
            else
            {
                Deployment.Current.Dispatcher.BeginInvoke(() => IsThreadActive = false);
                SwitchToMessageForm();
            }
            SetFormFieldDefaults();
            HideOverlay();
        }

        internal void SetFormFieldDefaults()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                var persistedValues = FeedbackManager.Instance.ThreadMetaInfos;
                this.Email = persistedValues.Email;
                this.Username = persistedValues.Username;
                this.Subject = persistedValues.Subject;
            });
        }

        #region Properties

        private string threadinfo;
        public string ThreadInfo
        {
            get { return threadinfo; }
            set
            {
                threadinfo = value;
                NotifyOfPropertyChange("ThreadInfo");
            }
        }


        private Boolean isMessageListVisible = true;
        public Boolean IsMessageListVisible
        {
            get { return isMessageListVisible; }
            protected set
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

        private bool isShowOverlay = true;
        public bool IsShowOverlay
        {
            get { return isShowOverlay; }
            set { 
                isShowOverlay = value;
                NotifyOfPropertyChange("IsShowOverlay");
            }
        }

        private bool isThreadActive = true;
        public bool IsThreadActive
        {
            get { return isThreadActive; }
            set
            {
                isThreadActive = value;
                NotifyOfPropertyChange("IsThreadActive");
            }
        }


        private string email;
        public string Email
        {
            get { return email; }
            set
            {
                email = value;
                NotifyOfPropertyChange("Email");
            }
        }

        private string subject;
        public string Subject
        {
            get { return subject; }
            set
            {
                subject = value;
                NotifyOfPropertyChange("Subject");
            }
        }

        private string message;
        public string Message
        {
            get { return message; }
            set
            {
                message = value;
                NotifyOfPropertyChange("Message");
            }
        }

        private string username;
        public string Username
        {
            get { return username; }
            set
            {
                username = value;
                NotifyOfPropertyChange("Username");
            }
        }

        #endregion

        ObservableCollection<FeedbackMessageVM> messages = new ObservableCollection<FeedbackMessageVM>();
        public ObservableCollection<FeedbackMessageVM> Messages
        {
            get { return messages; }
        }

        public void SwitchToMessageList()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                this.IsMessageListVisible = true;
                this.IsShowOverlay = false;
            });
            showFormAppBarAction(false);
        }

        public void SwitchToMessageForm()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => {
                this.IsMessageListVisible = false;
                this.IsShowOverlay = false;
            });
            showFormAppBarAction(true);
            
        }

        public void ShowOverlay()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => { this.IsShowOverlay = true; });
        }

        public void HideOverlay()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => { this.IsShowOverlay = false; });
        }

        protected bool ValidateInput()
        {
            var errors = new List<string>();
            if (!this.Email.IsValidEmail())
            {
                errors.Add("Please enter a valid email."); //TODO i18n
            }

            if (this.Subject.IsEmpty())
            {
                errors.Add("Please enter a subject."); //TODO i18n
            }

            if (this.Username.IsEmpty())
            {
                errors.Add("Please enter a name."); //TODO i18n
            }

            if (this.Message.IsEmpty())
            {
                errors.Add("Please enter a message."); //TODO i18n
            }

            if (errors.Any())
            {
                MessageBox.Show(errors.Aggregate((a, b) => a + "\n" + b));
                return false;
            }
            return true;
        }

        public async Task<bool> SubmitForm()
        {
            if (ValidateInput())
            {
                ShowOverlay();
                var msg = new FeedbackMessage()
                {
                    text = this.Message,
                    email = this.Email,
                    subject = this.Subject,
                    name = this.Username
                };

                var returnedMsg = await FeedbackManager.Instance.SendFeedback(msg, this.Username, this.Email);
                if (returnedMsg != null)
                {
                    this.Messages.Add(new FeedbackMessageVM(returnedMsg));
                    this.ClearForm();
                    SwitchToMessageList();
                    return true;
                }
                else
                {
                    HideOverlay();
                    MessageBox.Show("An error occcured. Please try again.");
                }
            }
            return false;
        }

        private void ClearForm()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                this.IsThreadActive = true;
                this.Message = "";
            });
        }
    }
}
