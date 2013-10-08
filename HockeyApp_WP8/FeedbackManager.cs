using System.Windows.Navigation;
using HockeyApp.Model;
using HockeyApp.Tools;
using System;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace HockeyApp
{
    public class FeedbackManager
    {

        private static readonly FeedbackManager instance = new FeedbackManager();
        private string identifier = null;
        private Application application = null;

        private string usernameInitial;
        private string emailInitial;

        static FeedbackManager() { }
        private FeedbackManager() { }

        public static FeedbackManager Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// Optional. Only needed if you don't configured a CrashHandler
        /// </summary>
        /// <param name="application"></param>
        /// <param name="appIdentifier"></param>
        public void Configure(Application application, string appIdentifier, string toptitle = null, string initialUsername = null, string initialEmail = null)
        {
            if (this.application == null)
            {
                this.identifier = appIdentifier;
                this.application = application;
                this.FeedbackPageTopTitle = toptitle;
                this.usernameInitial = initialUsername;
                this.emailInitial = initialEmail;
            }
            else
            {
                throw new InvalidOperationException("FeedbackManager was already configured!");
            }
        }

        public string FeedbackPageTopTitle { get; private set; }

        internal string AppIdentitfier { get { return this.identifier ?? CrashHandler.Instance.AppIdentitfier; } }
        internal Application Application { get { return this.application ?? CrashHandler.Instance.Application; } }

        public void NavigateToFeedbackUI(NavigationService navigationService)
        {
            navigationService.Navigate(new Uri("/HockeySDK;component/Views/FeedbackPage.xaml", UriKind.Relative));
        }

        private string threadToken;
        public string ThreadToken
        {
            get
            {
                IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
                return settings.GetValue(Constants.FeedbackThreadKey) as string;
            }
            set { threadToken = value; }
        }

        public bool IsThreadOpen
        {
            get
            {
                return this.ThreadToken != null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>FeedbackThread or null if the thread got deleted</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<FeedbackThread> GetActiveThreadAsync()
        {
            var request = WebRequest.CreateHttp(new Uri(Constants.ApiBase + "apps/" + this.AppIdentitfier + "/feedback/" + this.ThreadToken + ".json", UriKind.Absolute));
            request.Method = HttpMethod.Get;
            request.Headers[HttpRequestHeader.UserAgent] = Constants.UserAgentString;

            try
            {
                var response = await request.GetResponseTaskAsync();

#if WP8
                var fbResp = await Task.Run<FeedbackResponseSingle>(() => FeedbackResponseSingle.FromJson(response.GetResponseStream()));
#else
                var fbResp = await TaskEx.Run<FeedbackResponseSingle>(() => FeedbackResponseSingle.FromJson(response.GetResponseStream()));
#endif
                if (fbResp.status.Equals("success"))
                {
                    return fbResp.feedback;
                }
                else
                {
                    throw new Exception("Server error. Server returned status " + fbResp.status);
                }
            }
            catch (Exception e)
            {
                var webex = e.InnerException as WebException;
                if (webex != null)
                {
                    if (webex.Response.ContentType.IsEmpty())
                    {
                        //Connection error during call
                        throw webex;
                    }
                    else
                    {
                        //404 Response from server => thread got deleted
                        ForgetThreadInfos();
                        return null;
                    }
                }
                else
                {
                    throw;
                }
            }
        }

        public FeedbackThreadMetaInfos ThreadMetaInfos {
            get
            {
                IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
                return new FeedbackThreadMetaInfos(
                    settings.GetValue(Constants.FeedbackThreadSubjectKey) as string,
                    settings.GetValue(Constants.FeedbackUsernameKey) as string ?? this.usernameInitial ?? CrashHandler.Instance.UserId,
                    settings.GetValue(Constants.FeedbackEmailKey) as string ?? this.emailInitial ?? CrashHandler.Instance.ContactInfo,
                    settings.GetValue(Constants.FeedbackThreadKey) as string);
            }
        }

        protected void ForgetThreadInfos()
        {
            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
            settings.RemoveValue(Constants.FeedbackThreadKey);
            settings.RemoveValue(Constants.FeedbackThreadSubjectKey);
            settings.Save();
        }

        protected void PersistThreadMetaInfos(FeedbackResponseSingle fbResponse, string user, string email)
        {
            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
            settings.SetValue(Constants.FeedbackThreadKey, fbResponse.token);
            settings.SetValue(Constants.FeedbackThreadSubjectKey, fbResponse.feedback.messages.First().subject);
            settings.SetValue(Constants.FeedbackEmailKey, email);
            settings.SetValue(Constants.FeedbackUsernameKey, user);
            settings.Save();
        }

        protected async Task<FeedbackResponseSingle> PostAnswerAsync(FeedbackMessage message)
        {
            var request = WebRequest.CreateHttp(new Uri(Constants.ApiBase + "apps/" + this.AppIdentitfier + "/feedback/" + this.ThreadToken + "/", UriKind.Absolute));
            request.Method = HttpMethod.Put;
            request.Headers[HttpRequestHeader.UserAgent] = Constants.UserAgentString;
            await request.SetPostDataAsync(message.SerializeToWwwForm());

            var response = await request.GetResponseTaskAsync();
#if WP8
            var fbResp = await Task.Run<FeedbackResponseSingle>(() => FeedbackResponseSingle.FromJson(response.GetResponseStream()));
#else
            var fbResp = await TaskEx.Run<FeedbackResponseSingle>(() => FeedbackResponseSingle.FromJson(response.GetResponseStream()));
#endif

            if (fbResp.status.Equals("success"))
            {
                return fbResp;
            }
            else
            {
                throw new Exception("Server error. Server returned status " + fbResp.status);
            }
        }

        protected async Task<FeedbackResponseSingle> StartThreadAsync(FeedbackMessage message)
        {
            var request = WebRequest.CreateHttp(new Uri(Constants.ApiBase + "apps/" + this.AppIdentitfier + "/feedback", UriKind.Absolute));
            request.Method = HttpMethod.Post;
            request.Headers[HttpRequestHeader.UserAgent] = Constants.UserAgentString;
            await request.SetPostDataAsync(message.SerializeToWwwForm());

            var response = await request.GetResponseTaskAsync();
#if WP8
            var fbResp = await Task.Run<FeedbackResponseSingle>(() => FeedbackResponseSingle.FromJson(response.GetResponseStream()));
#else
            var fbResp = await TaskEx.Run<FeedbackResponseSingle>(() => FeedbackResponseSingle.FromJson(response.GetResponseStream()));
#endif

            if (fbResp.status.Equals("success"))
            {
                return fbResp;
            }
            else
            {
                throw new Exception("Server error. Server returned status " + fbResp.status);
            }
        }


        internal async Task<FeedbackMessage> SendFeedback(FeedbackMessage msg, string user = null, string email = null)
        {
            if (this.IsThreadOpen)
            {
                var fbResponse = await this.PostAnswerAsync(msg);
                if (fbResponse != null)
                {
                    return fbResponse.feedback.messages.Last();
                }
            }
            else
            {
                var fbResponse = await this.StartThreadAsync(msg);
                if (fbResponse != null)
                {
                    this.ThreadToken = fbResponse.token;
                    this.PersistThreadMetaInfos(fbResponse, user, email);
                    return fbResponse.feedback.messages.Last();
                }
            }
            return null;
        }
    }

}
