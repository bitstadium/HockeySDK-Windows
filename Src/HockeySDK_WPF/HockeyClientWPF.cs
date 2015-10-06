using HockeyApp;
using HockeyApp.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HockeyApp
{
    /// <summary>
    /// HockeyClient for WPF
    /// </summary>
    [Obsolete("Use HockeyClient.Current instead of HockeyClientWPF. All functions are available as Extension Methods on IHockeyClient")]
    public class HockeyClientWPF
    {
        private ILog logger = HockeyLogManager.GetLog(typeof(HockeyClientWPF));

        private static readonly HockeyClientWPF _instance = new HockeyClientWPF();

        /// <summary>
        /// Gets the instance of HockeyClientWPF.
        /// </summary>
        public static HockeyClientWPF Instance { get { return _instance; } }

        /// <summary>
        /// Gets path to hockey crashes.
        /// </summary>
        public string GetPathToHockeyCrashes { get { return Constants.GetPathToHockeyCrashes(); } }

        private CrashHandler _crashHandler = null;

        /// <summary>
        /// Configures the HockeyAppSDK
        /// </summary>
        /// <param name="appIdentifier">Identifier of the app</param>
        /// <param name="appVersionInformation">optional version of the app. If null the executing assembly version is used</param>
        /// <param name="userID">optional user id - e.g. the logged in user</param>
        /// <param name="contactInformation">optional contact information like an email adress</param>
        /// <param name="descriptionLoader">optional delegate for attaching description information like event logs etc. Can be null.</param>
        /// <param name="apiBase">optional: apiBase - if not the standard is used</param>
        /// <param name="userAgentString">The user agent string.</param>
        /// <param name="keepRunning">optional: if an unhandled exception is thrown on the dispatcher thread, the system can be kept alive - default is false</param>
        /// <exception cref="System.Exception">ApiBase must not be empty!</exception>
        public void Configure(string appIdentifier, 
                            string appVersionInformation = null,
                            string userID = null, 
                            string contactInformation = null,
                            Func<Exception, string> descriptionLoader = null,
                            string apiBase = "https://rink.hockeyapp.net",
                            string userAgentString = null,
                            bool keepRunning = false)
          
        {
            if (String.IsNullOrWhiteSpace(apiBase))
            {
                throw new Exception("ApiBase must not be empty!");
            }

            logger.Info("Configure HockeyClientWPF with appIdentifier={0}, userID={1}, contactInformation={2}, descriptionLoader available{3}, sendCrashesAutomatically={4}, apiBase={5}",
                new object[] { appIdentifier, userID, contactInformation, (descriptionLoader != null).ToString(),apiBase });

            string  myAppVersion= string.IsNullOrEmpty(appVersionInformation)?
                Assembly.GetCallingAssembly().GetName().Version.ToString()
                :appVersionInformation;

            HockeyClient.ConfigureInternal(appIdentifier,
                myAppVersion,
                apiBase: apiBase,
                userID: userID,
                contactInformation: contactInformation,
                userAgentName: Constants.USER_AGENT_STRING,
                sdkName: Constants.SDKNAME,
                sdkVersion: Constants.SDKVERSION,
                os: Environment.OSVersion.Platform.ToString(),
                osVersion: Environment.OSVersion.Version.ToString() + Environment.OSVersion.ServicePack);
            this._crashHandler = new CrashHandler(HockeyClient.Instance, descriptionLoader, keepRunning);
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
        public async Task SendCrashesNowAsync() {await this._crashHandler.SendCrashesNowAsync(); }

        /// <summary>
        /// Cleans crashes.
        /// </summary>
        public void DeleteAllCrashes()
        {
            this._crashHandler.DeleteAllCrashes();
        }

        #endregion

        #region Feedback
        /// <summary>
        /// Creates a new Feedback-Thread. Thread is stored on the server with the first message.
        /// </summary>
        /// <returns></returns>
        public IFeedbackThread CreateFeedbackThread()
        {
            return HockeyClient.Current.AsInternal().CreateNewFeedbackThread();
        }

        /// <summary>
        /// Opens a Feedback-Thread on the server.
        /// </summary>
        /// <param name="feedbackToken">A guid which identifies the Feedback-Thread</param>
        /// <returns>The Feedback-Thread or, if not found or delete, null.</returns>
        public async Task<IFeedbackThread> OpenFeedbackThreadAsync(string feedbackToken)
        {
            return await HockeyClient.Current.AsInternal().OpenFeedbackThreadAsync(feedbackToken);
        }

        #endregion

        #region Update
        private IUpdateManager _updateManager = null;

        /// <summary>
        /// Gets update manager.
        /// </summary>
        public IUpdateManager UpdateManager {
            get
            {
                if (this._updateManager == null)
                {
                    this._updateManager = new UpdateManager();
                }
                return this._updateManager;
            }
        }
        #endregion
    }
}
