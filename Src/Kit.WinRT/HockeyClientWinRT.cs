using Microsoft.HockeyApp.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.HockeyApp;

namespace Microsoft.HockeyApp
{
    [Obsolete("Please use HockeyClient.Current and applicable Extension methods")]
    public class HockeyClientWinRT
    {
        private ILog logger = HockeyLogManager.GetLog(typeof(HockeyClientWinRT));

        private static readonly HockeyClientWinRT _instance = new HockeyClientWinRT();
        public static HockeyClientWinRT Instance { get { return _instance; } }

        private CrashHandler _crashHandler = null;

        /// <summary>
        /// Configures the HockeyAppSDK
        /// </summary>
        /// <param name="appIdentifier">Identifier of the app</param>
        /// <param name="appVersionInformation">version of the app</param>
        /// <param name="userID">optional user id - e.g. the logged in user</param>
        /// <param name="contactInformation">optional contact information like an email adress</param>
        /// <param name="descriptionLoader">optional delegate for attaching description information like event logs etc. Can be null.</param>
        /// <param name="apiBase">optional: apiBase - if not the standard is used</param>
        public void Configure(string appIdentifier, 
                            string appVersionInformation,
                            string userID = null, 
                            string contactInformation = null,
                            Func<Exception, string> descriptionLoader = null,
                            string apiBase = "https://rink.hockeyapp.net",
                            string userAgentString = null)
          
        {
            if (String.IsNullOrWhiteSpace(apiBase))
            {
                throw new Exception("ApiBase must not be empty!");
            }

            logger.Info("Configure HockeyClientWPF with appIdentifier={0}, userID={1}, contactInformation={2}, descriptionLoader available{3}, sendCrashesAutomatically={4}, apiBase={5}",
                new object[] { appIdentifier, userID, contactInformation, (descriptionLoader != null).ToString(),apiBase });

            HockeyClient.ConfigureInternal(appIdentifier,
                appVersionInformation,
                apiBase: apiBase,
                userID: userID,
                contactInformation: contactInformation,
                userAgentName: "",
                sdkName: Constants.SDKNAME,
                sdkVersion: Constants.SDKVERSION);

            WindowsAppInitializer.InitializeAsync(userID);
            this._crashHandler = new CrashHandler(HockeyClient.Instance, descriptionLoader);
        }

        #region Crashes
        /// <summary>
        /// Returns, if not sent crashes are available
        /// </summary>
        public bool CrashesAvailable { get { return this._crashHandler.CrashesAvailable; } }

        /// <summary>
        /// returns the amount of crashes, which are not sent
        /// </summary>
        public int CrashesAvailableCount { get { return this._crashHandler.CrashesAvailableCount; } }

        /// <summary>
        /// Sends all available crashes
        /// </summary>
        /// <returns>Task</returns>
        public async Task SendCrashesNow() {await this._crashHandler.SendCrashesNowAsync(); }

        public Task DeleteAllCrashesAsync()
        {
            return this._crashHandler.DeleteAllCrashesAsync();
        }

        #endregion

        #region Feedback
        /// <summary>
        /// Creates a new Feedback-Thread. Thread is stored on the server with the first message.
        /// </summary>
        /// <returns></returns>
        public IFeedbackThread CreateFeedbackThread()
        {
            return ((IHockeyClientInternal)HockeyClient.Current).CreateNewFeedbackThread();
        }

        /// <summary>
        /// Opens a Feedback-Thread on the server.
        /// </summary>
        /// <param name="feedbackToken">A guid which identifies the Feedback-Thread</param>
        /// <returns>The Feedback-Thread or, if not found or delete, null.</returns>
        public async Task<IFeedbackThread> OpenFeedbackThreadAsync(string feedbackToken)
        {
            return await ((IHockeyClientInternal)HockeyClient.Current).OpenFeedbackThreadAsync(feedbackToken);
        }

        #endregion
    }
}
