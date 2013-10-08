using HockeyApp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HockeyApp
{
    public class HockeyClient : HockeyApp.IHockeyClient
    {
        

        #region fields

        //Platform and communication info
        public string ApiBase { get; private set; }
        public string UserAgentString { get; set; }

        //SDK info
        public string SdkName { get; private set; }
        public string SdkVersion { get; private set; }

        //App info
        public string AppIdentifier { get; private set; }
        public string VersionInformation { get; private set; }
        public string UserID { get; private set; }
        public string ContactInformation { get; private set; }

        #endregion


        #region ctor
        private ILog _logger = HockeyLogManager.GetLog(typeof(HockeyClient));
        private static HockeyClient _instance=null;

        public static void Configure(string apiBase, 
                                        string userAgentName,
                                        string sdkName, 
                                        string sdkVersion,
                                        string appIdentifier, 
                                        string versionInformation,
                                        string userID, 
                                        string contactInformation){

            _instance = new HockeyClient();
            _instance.UserAgentString = userAgentName;
            _instance.AppIdentifier = appIdentifier;
            _instance.VersionInformation = versionInformation;
            _instance.UserID = userID;
            _instance.ContactInformation = contactInformation;
            _instance.ApiBase = apiBase;
            if (!_instance.ApiBase.EndsWith("/")) { _instance.ApiBase += "/"; }
            _instance.SdkName = sdkName;
            _instance.SdkVersion = sdkVersion;
        }


        public static IHockeyClient Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new Exception("HockeyClient not configured. Call HockeyClient.Configure first!");
                }
                return _instance;
            }
        }
        private HockeyClient(){}

        #endregion

        #region API functions
        /// <summary>
        /// Posts a crash to the HockeyApp-Server
        /// </summary>
        /// <param name="log">Modelinformation and the stack trace - see API Documentation of Hockey App</param>
        /// <param name="userID">optional: the user, which was logged in</param>
        /// <param name="contactInformation">optional: contact information</param>
        /// <param name="description">optional: a description or excerpt of any event logs</param>
        /// <returns></returns>
        ///
        public async Task PostCrashAsync(string log, string userID="", string contactInformation="", string description="")
        {
            CrashData cd = new CrashData(this);
            cd.Log = log;
            cd.UserID = userID;
            cd.Contact = contactInformation;
            cd.Description = description;
            await cd.SendData();
        }


        public async Task<Model.AppVersion> GetLatestAppVersion()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a new Feedback-Thread. The thread is stored on the server with the first posted message.
        /// </summary>
        /// <returns></returns>
        public IFeedbackThread CreateNewFeedbackThread()
        {
            return FeedbackThread.CreateInstance();
        }

        /// <summary>
        /// Opens an existing Feedback-Thread on the server using the Thread-Token.
        /// </summary>
        /// <param name="threadToken">The Feedback-Thread or null, if the thread is not available or deleted on the server.</param>
        /// <returns></returns>
        public async Task<IFeedbackThread> OpenFeedbackThreadAsync(string threadToken)
        {
            if (String.IsNullOrWhiteSpace(threadToken))
            {
                throw new ArgumentException("Token must not be empty!");
            }
            FeedbackThread fbThread = null;
            fbThread = await FeedbackThread.OpenFeedbackThreadAsync(this, threadToken);
            return fbThread;
        }

        #endregion

    }
}
