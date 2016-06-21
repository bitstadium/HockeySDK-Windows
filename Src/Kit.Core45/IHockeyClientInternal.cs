namespace Microsoft.HockeyApp.Internal
{
    using Model;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// Full interface of Hockeyclient. Used by platform-specific SDKs
    /// </summary>
    internal interface IHockeyClientInternal : IHockeyClient
    {
        #region Properties

        /// <summary>
        /// Base-URI of Hockey Server API v2
        /// </summary>
        string ApiBaseVersion2 { get; }

        /// <summary>
        /// Base-URI of Hockey Server API v2
        /// </summary>
        string ApiBaseVersion3 { get; }

        /// <summary>
        /// domain of HockeyApp server
        /// </summary>
        string ApiDomain { get; set; }

        /// <summary>
        /// User-agent string used in server communication
        /// </summary>
        string UserAgentString { get; set; }

        /// <summary>
        /// Name of SDK
        /// </summary>
        string SdkName { get; set; }
        /// <summary>
        /// SDK Version
        /// </summary>
        string SdkVersion { get; set; }

        /// <summary>
        /// Public identifier of you app
        /// </summary>
        string AppIdentifier { get; set; }
        /// <summary>
        /// Current version of your app
        /// </summary>
        string VersionInfo { get; set; }
        /// <summary>
        /// User Id to be sent with crash reports
        /// </summary>
        string UserID { get; set; }
        /// <summary>
        /// Contact information to be sent with crash reports
        /// </summary>
        string ContactInformation { get; set; }

        /// <summary>
        /// DescriptionLoader Func which is called for unhandled exceptions. the returned string is added to the crashlog as description.
        /// </summary>
        Func<Exception, string> DescriptionLoader { get; set; }

        bool IsTelemetryInitialized { get; set; }

        #endregion

        /// <summary>
        /// Handle Exceptions that are swallowed because we don't want our SDK crash other apps
        /// For internal use by platform SDKs
        /// </summary>
        /// <param name="e"></param>
        void HandleInternalUnhandledException(Exception e);

        #region PlatformHelper

        /// <summary>
        /// Platformhelper implemented by platform-specific SDKs
        /// </summary>
        IHockeyPlatformHelper PlatformHelper { get; set; }
        /// <summary>
        /// crsh log meta information prefilled with data from platform helper
        /// </summary>
        CrashLogInformation PrefilledCrashLogInfo { get; }

        #endregion

        /// <summary>
        /// Check if this IHockeyClient has already been initialized. throws exception if not initialized.
        /// </summary>
        void CheckForInitialization();

        #region API calls
        /// <summary>
        /// Authenticate a user against hockeyapp.
        /// The returned IAuthStatus can be serialized and saved to later check if the token is still valid.
        /// </summary>
        /// <param name="email">email of the user</param>
        /// <param name="password">password of the user</param>
        /// <returns>an IAuthStatus containing the auid-token (if login is valid)</returns>
        Task<IAuthStatus> AuthorizeUserAsync(string email, string password);

        /// <summary>
        /// Identify a user against hockeyapp.
        /// The returned IAuthStatus can be serialized and saved to later check if the token is still valid.
        /// </summary>
        /// <param name="email">email of the user</param>
        /// <param name="appSecret">appSecret of your app</param>
        /// <returns>an IAuthStatus containing the auid-token (if login is valid)</returns>
        Task<IAuthStatus> IdentifyUserAsync(string email, string appSecret);

        /// <summary>
        /// Creates a new Feedback-Thread. The thread is stored on the server with the posting of the first message.
        /// </summary>
        /// <returns></returns>
        IFeedbackThread CreateNewFeedbackThread();

        /// <summary>
        /// Opens an existing Feedback-Thread on the server using the Thread-Token.
        /// </summary>
        /// <param name="threadToken">The Feedback-Thread or null, if the thread is not available or deleted on the server.</param>
        /// <returns></returns>
        Task<IFeedbackThread> OpenFeedbackThreadAsync(string threadToken);

        /// <summary>
        /// Retrieves the current AppVersion from the HockeyApp-Server
        /// </summary>
        /// <returns>Metadata of the newest version of the app</returns>
        Task<IEnumerable<IAppVersion>> GetAppVersionsAsync();
        #endregion


        #region Crash handling

        /// <summary>
        /// Factory method for ICrashData
        /// </summary>
        /// <param name="ex">Exception for which crashData is created</param>
        /// <param name="crashLogInfo">Meta infos for crash</param>
        /// <returns></returns>
        ICrashData CreateCrashData(Exception ex, CrashLogInformation crashLogInfo);

        /// <summary>
        /// Factory method for ICrashData
        /// </summary>
        /// <param name="ex">Exception for which crashData is created</param>
        /// <returns></returns>
        ICrashData CreateCrashData(Exception ex);

        /// <summary>
        /// Factory method for ICrashData (for unity-sdk)
        /// </summary>
        /// <param name="logString">The log string.</param>
        /// <param name="stackTrace">The stack trace.</param>
        /// <returns></returns>
        ICrashData CreateCrashData(String logString, String stackTrace);


        /// <summary>
        /// Deserializes an ICrashData from stream information
        /// </summary>
        /// <param name="inputStream"></param>
        /// <returns></returns>
        ICrashData Deserialize(Stream inputStream);

        
        /// <summary>
        /// Returns filenames of current existing Crashlogs
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<string>> GetCrashFileNamesAsync();

        /// <summary>
        /// Delete all existing Crash-logs
        /// </summary>
        /// <returns></returns>
        Task DeleteAllCrashesAsync();
        
        /// <summary>
        /// Indicates if any crash-logs are available in storage
        /// </summary>
        /// <returns></returns>
        Task<bool> AnyCrashesAvailableAsync();

        /// <summary>
        /// Handle Exception async
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        Task HandleExceptionAsync(Exception ex);

        /// <summary>
        /// Handle Exception sync (only on platforms that support sync file access)
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        void HandleException(Exception ex);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<bool> SendCrashesAndDeleteAfterwardsAsync();

        #endregion


    }
}
