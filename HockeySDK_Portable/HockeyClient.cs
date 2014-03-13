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
        [Obsolete("Use Version-specific ApiBase!")]
        public string ApiBase
        {
            get { return ApiBaseVersion2; }
            private set
            {
                if (!String.IsNullOrEmpty(value) )
                {
                    string domain = value;
                    //needed for backwards compatibility
                    if (value.Contains("/api/"))
                    {
                        domain = value.Substring(0, value.IndexOf("/api/") + 1);
                    }
                    ApiDomain = domain.EndsWith("/") ? domain : domain + "/";
                }
                else
                {
                    ApiDomain = value;
                }
            }
        }

        internal String ApiDomain { get; private set; }

        public string ApiBaseVersion2
        {
            get { return ApiDomain + "api/2/"; }
        }

        public string ApiBaseVersion3
        {
            get { return ApiDomain + "api/3/"; }
        }

        //User agent string (set by platform-specific SDK)
        public string UserAgentString { get; set; }

        //SDK info (set by platform-specific SDK if used)
        public string SdkName { get; private set; }
        //SDK Version (set by platform-specific SDK if used)
        public string SdkVersion { get; private set; }

        //App info
        public string AppIdentifier { get; private set; }
        //Current Version of the app as string
        public string VersionInfo { get; private set; }
        //UserID of current user
        public string UserID { get; set; }
        //Contact information for current user
        public string ContactInformation { get; set; }
        //Operating system (set by platform-specific SDK if used)
        public string Os { get; set; }
        //Operating system version (set by platform-specific SDK if used)
        public string OsVersion { get; set; }
        //Device (set by platform-specific SDK if used)
        public string Device { get; set; }
        //Oem of Device (set by platform-specific SDK if used)
        public string Oem { get; set; }
        //uniques user id provided by platform (set by platform-specific SDK if used)
        public string Uuid { get; set; }
        //Authorized user id (set during login process)
        public string Auid { get; internal set; }
        //Identified user id (set during login process)
        public string Iuid { get; internal set; }
        
        //Delegate which can be set to add a description to a stacktrace when app crashes
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
                                        Func<Exception, string> descriptionLoader = null,
                                        string os = null,
                                        string osVersion = null,
                                        string device = null,
                                        string oem = null,
                                        string uuid = null)
        {
            _instance = new HockeyClient();
            _instance.AppIdentifier = appIdentifier;
            _instance.VersionInfo = versionInfo;
            _instance.UserID = userID;
            _instance.ContactInformation = contactInformation;
            #pragma warning disable 618 // disable obsolete warning!
            _instance.ApiBase = apiBase ?? SDKConstants.PublicApiBase;
            #pragma warning disable 618
            _instance.UserAgentString = userAgentName ?? SDKConstants.UserAgentString;
            _instance.SdkName = sdkName ?? SDKConstants.SdkName;
            _instance.SdkVersion = sdkVersion ?? SDKConstants.SdkVersion;
            _instance.Os = os;
            _instance.OsVersion = os;
            _instance.Device = device;
            _instance.Oem = oem;
            _instance.Uuid = uuid;
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

        #region Crashes

        public ICrashData CreateCrashData(Exception ex, CrashLogInformation crashLogInfo)
        {
            return new CrashData(this, ex, crashLogInfo);
        }

        public ICrashData Deserialize(Stream inputStream)
        {
            return CrashData.Deserialize(inputStream);
        }
        #endregion

        #region Update

        public async Task<IEnumerable<IAppVersion>> GetAppVersionsAsync()
        {
            StringBuilder url = new StringBuilder(this.ApiBaseVersion2 + "apps/" + this.AppIdentifier + ".json");

            url.Append("?app_version=" + Uri.EscapeDataString(this.VersionInfo));
            url.Append("&os=" + Uri.EscapeDataString(this.Os));
            url.Append("&os_version=" + Uri.EscapeDataString(this.OsVersion));
            url.Append("&device=" + Uri.EscapeDataString(this.Device));
            url.Append("&=oem" + Uri.EscapeDataString(this.Oem));
            url.Append("&=sdk" + Uri.EscapeDataString(this.SdkName));
            url.Append("&=sdk_version" + Uri.EscapeDataString(this.SdkVersion));
            url.Append("&=lang" + Uri.EscapeDataString(System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName));

            if (!String.IsNullOrEmpty(this.Auid))
            {
                url.Append("&=lang" + Uri.EscapeDataString(System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName));
            }
            else if (!String.IsNullOrEmpty(this.Iuid))
            {
                url.Append("&=auid" + Uri.EscapeDataString(this.Iuid));
            }
            else if (!String.IsNullOrEmpty(this.Uuid))
            {
                url.Append("&=duid" + Uri.EscapeDataString(this.Uuid));
            }

            var request = WebRequest.CreateHttp(new Uri(this.ApiBaseVersion2 + "apps/" + this.AppIdentifier + ".json", UriKind.Absolute));
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

        #endregion

        #region Feedback

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

        #region Authentication

        private void FillEmptyUserAndContactInfo(string email)
        {
            if (String.IsNullOrEmpty(this.UserID))
            {
                this.UserID = email;
            }
            if (String.IsNullOrEmpty(this.ContactInformation))
            {
                this.ContactInformation = email;
            }
        }

        public async Task<IAuthStatus> AuthorizeUser(string email, string password)
        {
            var request = WebRequest.CreateHttp(new Uri(HockeyClient.Instance.ApiBaseVersion3 + "apps/" +
                                                           HockeyClient.Instance.AppIdentifier + "/identity/authorize", UriKind.Absolute));
            
            request.SetHeader(HttpRequestHeader.UserAgent.ToString(), HockeyClient.Instance.UserAgentString);
            byte[] credentialBuffer = new UTF8Encoding().GetBytes(email + ":" + password);
            request.SetHeader(HttpRequestHeader.Authorization.ToString(), "Basic " + Convert.ToBase64String(credentialBuffer));
            request.Method = "POST";
            var status = await AuthStatus.DoAuthRequestHandleResponse(request);
            if (status.IsAuthorized)
            {
                this.Auid = (status as AuthStatus).Auid;
                this.FillEmptyUserAndContactInfo(email);
            }
            return status;
        }

        public async Task<IAuthStatus> IdentifyUser(string email, string appSecret)
        {
            var request = WebRequest.CreateHttp(new Uri(HockeyClient.Instance.ApiBaseVersion3 + "apps/" +
                                                           HockeyClient.Instance.AppIdentifier + "/identity/check", UriKind.Absolute));
            request.SetHeader(HttpRequestHeader.UserAgent.ToString(), HockeyClient.Instance.UserAgentString);
            request.Method = "POST";

            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.UTF8.GetBytes("\r\n--" + boundary + "\r\n");

            var fields = new Dictionary<string, byte[]>();

            fields.Add("authcode", Encoding.UTF8.GetBytes((appSecret + email).GetMD5HexDigest()));
            fields.Add("email", Encoding.UTF8.GetBytes(email));
            
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            Stream stream = await request.GetRequestStreamAsync();
            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n";

            //write form fields
            foreach (var keyValue in fields)
            {
                stream.Write(boundarybytes, 0, boundarybytes.Length);
                string formitem = string.Format(formdataTemplate, keyValue.Key);
                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                stream.Write(formitembytes, 0, formitembytes.Length);
                stream.Write(keyValue.Value, 0, keyValue.Value.Length);
            }

            byte[] trailer = System.Text.Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");
            stream.Write(trailer, 0, trailer.Length);
            stream.Dispose();
            var status = await AuthStatus.DoAuthRequestHandleResponse(request);
            if (status.IsIdentified) {
                this.Iuid = (status as AuthStatus).Iuid;
                this.FillEmptyUserAndContactInfo(email); 
            }
            return status;
        }

        #endregion

        #endregion
    }
}