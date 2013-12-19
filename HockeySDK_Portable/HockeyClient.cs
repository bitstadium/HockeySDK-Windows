using HockeyApp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HockeyApp.Extensions;
using HockeyApp.Exceptions;
using System.IO;

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
        public string VersionInfo { get; private set; }
        public string UserID { get; set; }
        public string ContactInformation { get; set; }

        //for crashes:
        public Func<Exception, string> _descriptionLoader = null;

        #endregion


        #region ctor
        private ILog _logger = HockeyLogManager.GetLog(typeof(HockeyClient));
        private static HockeyClient _instance=null;

        /// <summary>
        /// Configures the HockeyClient with your app specific information
        /// </summary>
        /// <param name="appIdentifier">public identfier of your app (AppId)</param>
        /// <param name="versionInfo">version of your app</param>
        /// <param name="apiBase">[optional] the base url of the hockeyapp server. Only needed if used with a private HockeyApp installation.</param>
        /// <param name="userID">[optional] ID of the current user using your app, sent with crash-reports, can also be set via property.</param>
        /// <param name="contactInformation">[optional] contact info of the current user using your app, sent with crash-reports, can also be set via property.</param>
        public static void Configure(string appIdentifier,
                                        string versionInfo,
                                        string apiBase = null,
                                        string userID = null,
                                        string contactInformation = null,
                                        Func<Exception, string> descriptionLoader = null)
        {
            ConfigureInternal(appIdentifier, versionInfo, apiBase, userID, contactInformation, null, null, null ,descriptionLoader);
        }

        /// <summary>
        /// Use for advanced usecases like building your own platform specific sdk based on HockeyClient
        /// </summary>
        /// <param name="appIdentifier">public identfier of your app (AppId)</param>
        /// <param name="versionInfo">version of your app</param>
        /// <param name="apiBase">[optional] the base url of the hockeyapp server. Only needed if used with a private HockeyApp installation.</param>
        /// <param name="userID">[optional] ID of the current user using your app, sent with crash-reports, can also be set via property.</param>
        /// <param name="contactInformation">[optional] contact info of the current user using your app, sent with crash-reports, can also be set via property.</param>
        /// <param name="userAgentName">[optional] useragent string to be used in communication with the HockeyApp server</param>
        /// <param name="sdkName">[optional] name of the calling sdk</param>
        /// <param name="sdkVersion">[optional] version of the calling sdk </param>
        public static void ConfigureInternal(string appIdentifier,
                                        string versionInfo,
                                        string apiBase = null,
                                        string userID = null,
                                        string contactInformation = null,
                                        string userAgentName = null,
                                        string sdkName = null,
                                        string sdkVersion = null,
                                        Func<Exception, string> descriptionLoader = null)
        {
            _instance = new HockeyClient();
            _instance.AppIdentifier = appIdentifier;
            _instance.VersionInfo = versionInfo;
            _instance.UserID = userID;
            _instance.ContactInformation = contactInformation;
            _instance.ApiBase = apiBase ?? SDKConstants.PublicApiBase;
            _instance.UserAgentString = userAgentName ?? SDKConstants.UserAgentString;
            if (!_instance.ApiBase.EndsWith("/")) { _instance.ApiBase += "/"; }
            _instance.SdkName = sdkName ?? SDKConstants.SdkName;
            _instance.SdkVersion = sdkVersion ?? SDKConstants.SdkVersion;
        }

        /// <summary>
        /// The current configured instance of HockeyClient
        /// </summary>
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

        public ICrashData CreateCrashData(Exception ex, string crashLogInfo)
        {
            return new CrashData(this, ex, crashLogInfo);
        }

        public ICrashData Deserialize(Stream inputStream)
        {
            return CrashData.Deserialize(inputStream);
        }

        public async Task<IEnumerable<IAppVersion>> GetAppVersionsAsync()
        {
            var request = WebRequest.CreateHttp(new Uri(this.ApiBase + "apps/" + this.AppIdentifier + ".json", UriKind.Absolute));
            request.Method = "Get";
            request.SetHeader(HttpRequestHeader.UserAgent.ToString(), this.UserAgentString);
            var response = await request.GetResponseAsync();
            IEnumerable<AppVersion> appVersions = await TaskEx.Run(() => AppVersion.FromJson(response.GetResponseStream()));
            foreach (var ver in appVersions)
            {
                ver.PublicIdentifier = this.AppIdentifier; //the json response does not include the public app identifier
            }
            return appVersions;
        }
      
        public IFeedbackThread CreateNewFeedbackThread()
        {
            return FeedbackThread.CreateInstance();
        }

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