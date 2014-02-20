using System.Windows.Navigation;
using HockeyApp.Tools;
using System;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.Generic;
using HockeyApp.Model;
using System.Windows.Media.Imaging;
using System.IO;

namespace HockeyApp
{
    public class FeedbackManager
    {
        #region singleton

        private static readonly FeedbackManager instance = new FeedbackManager();
        static FeedbackManager() { }
        private FeedbackManager() { }

        public static FeedbackManager Instance
        {
            get { return instance; }
        }

        #endregion

        public string FeedbackPageTopTitle { get; private set; }
        private string usernameInitial;
        private string emailInitial;

        private string threadToken;
        private IFeedbackThread activeThread;

        /// <summary>
        /// Optional. Only needed if you want to set an initial toptitle for the UI or an intitial email and username.
        /// A Crashhandler has to be configured before usage of the Feedbackmanager!
        /// </summary>
        /// <param name="toptitle">Title shown over the header on the feedback page</param>
        /// <param name="initialUsername">Initial username to show in form</param>
        /// <param name="initialEmail">Initial email to show in form</param>
        public void Configure(string toptitle = null, string initialUsername = null, string initialEmail = null)
        {
            this.FeedbackPageTopTitle = toptitle;
            this.usernameInitial = initialUsername;
            this.emailInitial = initialEmail;
        }

        /// <summary>
        /// Navigates to the feedback page
        /// </summary>
        /// <param name="navigationService"></param>
        public void NavigateToFeedbackUI(NavigationService navigationService)
        {
            navigationService.Navigate(new Uri("/HockeyApp;component/Views/FeedbackPage.xaml", UriKind.Relative));
        }
        
        /// <summary>
        /// The token of the open feedback thread
        /// (you should not need this is you use the provided feedpage page)
        /// </summary>
        public string ThreadToken
        {
            get
            {
                IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
                return settings.GetValue(Constants.FeedbackThreadKey) as string;
            }
            set { threadToken = value; }
        }

        /// <summary>
        /// Indicates if a thread has already been opened by the app
        /// (you should not need this is you use the provided feedpage page)
        /// </summary>
        public bool IsThreadOpen
        {
            get
            {
                return this.ThreadToken != null;
            }
        }

        /// <summary>
        /// Gets the active feedback thread with all messages from the HockeyApp server (cached)
        /// (you should not need this is you use the provided feedpage page)
        /// </summary>
        /// <param name="forceReload">[optional] force reload of thread messages</param>
        /// <returns>the FeedbackThread</returns>
        public async Task<IFeedbackThread> GetActiveThreadAsync(bool forceReload = false)
        {
            if (this.activeThread != null && !forceReload) { return activeThread; }
            if (this.ThreadToken == null) { return null; }
            var thread = await HockeyClient.Instance.OpenFeedbackThreadAsync(this.ThreadToken);
            if (thread == null)
            {
                //thread got deleted
                ForgetThreadInfos();
            }
            else
            {
                this.activeThread = thread;
            }
            return thread;
        }

        /// <summary>
        /// Get saved Feedbackthread-metadata 
        /// (you should not need this is you use the provided feedpage page)
        /// </summary>
        public FeedbackThreadMetaInfos ThreadMetaInfos {
            get
            {
                IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
                return new FeedbackThreadMetaInfos(
                    settings.GetValue(Constants.FeedbackThreadSubjectKey) as string,
                    settings.GetValue(Constants.FeedbackUsernameKey) as string ?? this.usernameInitial ?? HockeyClient.Instance.UserID,
                    settings.GetValue(Constants.FeedbackEmailKey) as string ?? this.emailInitial ?? HockeyClient.Instance.ContactInformation,
                    settings.GetValue(Constants.FeedbackThreadKey) as string);
            }
        }

        /// <summary>
        /// Deletes all persistently stored data like FeedbackThreadToken, UserName, etc. Call in your app if user logs out.
        /// </summary>
        public void Logout()
        {
            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
            settings.RemoveValue(Constants.FeedbackThreadKey);
            settings.RemoveValue(Constants.FeedbackThreadSubjectKey);
            settings.RemoveValue(Constants.FeedbackEmailKey);
            settings.RemoveValue(Constants.FeedbackUsernameKey);
            settings.Save();            
        }

        protected void ForgetThreadInfos()
        {
            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
            settings.RemoveValue(Constants.FeedbackThreadKey);
            settings.RemoveValue(Constants.FeedbackThreadSubjectKey);
            settings.Save();
        }

        protected void PersistThreadMetaInfos(string token, string subject, string user, string email)
        {
            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
            settings.SetValue(Constants.FeedbackThreadKey, token);
            settings.SetValue(Constants.FeedbackThreadSubjectKey, subject);
            settings.SetValue(Constants.FeedbackEmailKey, email);
            settings.SetValue(Constants.FeedbackUsernameKey, user);
            settings.Save();
        }

        /// <summary>
        /// Send a feedback message to the server
        /// (you should not need this if you use the provided feedback page)
        /// </summary>
        /// <param name="message">message text</param>
        /// <param name="email">email address of sender</param>
        /// <param name="subject">subject of message</param>
        /// <param name="name">name of sender</param>
        /// <returns></returns>
        public async Task<IFeedbackMessage> SendFeedback(string message, string email, string subject, string name, IEnumerable<IFeedbackAttachment> attachments)
        {
            var thread = await this.GetActiveThreadAsync() ?? FeedbackThread.CreateInstance();

            foreach (var attachment in attachments)
            {
                //convert all images to jpg for filesize
                var bitimg = new BitmapImage();
                bitimg.SetSource(new MemoryStream(attachment.DataBytes));
                var wb = new WriteableBitmap(bitimg);

                using (var stream = new MemoryStream())
                {
                    wb.SaveJpeg(stream, bitimg.PixelWidth, bitimg.PixelHeight, 0, 70);
                    stream.Seek(0, System.IO.SeekOrigin.Begin);
                    var buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, (int)stream.Length);
                    attachment.DataBytes = buffer;
                }
                attachment.FileName = Path.GetFileNameWithoutExtension(attachment.FileName) + ".jpg";
                attachment.ContentType = "image/jpeg";
            }

            IFeedbackMessage msg;
            try
            {
                msg = await thread.PostFeedbackMessageAsync(message, email, subject, name, attachments);
                PersistThreadMetaInfos(thread.Token, subject, name, email);
                this.activeThread = thread;
            }
            catch (Exception)
            {
                this.activeThread = null;
                throw;
            }
            return msg;
        }
    }

}
