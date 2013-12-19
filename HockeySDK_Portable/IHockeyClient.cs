using HockeyApp.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
namespace HockeyApp
{
    public interface IHockeyClient
    {
        /// <summary>
        /// Base-URI of Hockey Server 
        /// </summary>
        string ApiBase { get; }
        /// <summary>
        /// User-agent string used in server communication
        /// </summary>
        string UserAgentString { get; }

        /// <summary>
        /// Name of SDK
        /// </summary>
        string SdkName { get; }
        /// <summary>
        /// SDK Version
        /// </summary>
        string SdkVersion { get; }

        /// <summary>
        /// Public identifier of you app
        /// </summary>
        string AppIdentifier { get; }
        /// <summary>
        /// Current version of your app
        /// </summary>
        string VersionInfo { get; }
        /// <summary>
        /// 
        /// </summary>
        string UserID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        string ContactInformation { get; set; }


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

        /// <summary>
        /// Factory Method for ICrashData
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="crashLogInfo"></param>
        /// <returns></returns>
        ICrashData CreateCrashData(Exception ex, string crashLogInfo);

        /// <summary>
        /// Deserializes an ICrashData from stream information
        /// </summary>
        /// <param name="inputStream"></param>
        /// <returns></returns>
        ICrashData Deserialize(Stream inputStream);
    }
}
