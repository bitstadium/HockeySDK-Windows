using HockeyApp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using HockeyApp.Exceptions;
using HockeyApp.Extensions;
using HockeyApp.Internal;
using System.Net.NetworkInformation;
using System.Threading;

namespace HockeyApp
{
    public class HockeyClient : HockeyApp.IHockeyClient, IHockeyClientInternal, IHockeyClientConfigurable
    {
        private ILog logger = HockeyLogManager.GetLog(typeof(HockeyClient));

        #region fields

        //Platform and communication info
        [Obsolete("Use Version-specific ApiBase!")]
        public string ApiBase
        {
            get { return ApiBaseVersion2; }
            private set
            {
                if (!String.IsNullOrEmpty(value))
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

        private string _apiDomain = SDKConstants.PublicApiBase + "/";
        public string ApiDomain
        {
            get { return _apiDomain; }
            set
            {
                if (value != null)
                {
                    _apiDomain = value.EndsWith("/") ? value : value + "/";
                }
            }
        }

        public string ApiBaseVersion2
        {
            get { return ApiDomain + "api/2/"; }
        }

        public string ApiBaseVersion3
        {
            get { return ApiDomain + "api/3/"; }
        }

        //User agent string (set by platform-specific SDK)
        private string _userAgentString;
        public string UserAgentString
        {
            get {
                if (_userAgentString == null)
                {
                    this._userAgentString = SDKConstants.UserAgentString;
                    if (this.PlatformHelper != null)
                    {
                        this._userAgentString = this.PlatformHelper.UserAgentString;
                    }
                }
                return _userAgentString; }
            set { _userAgentString = value; }
        }

        //SDK info (set by platform-specific SDK if used)
        private string _sdkName;
        public string SdkName
        {
            get {
                if (_sdkName == null)
                {
                    this._sdkName = SDKConstants.SdkName;
                    if (this.PlatformHelper != null)
                    {
                        this._sdkName = this.PlatformHelper.SDKName;
                    }
                }
                return _sdkName; }
            set { _sdkName = value; }
        }

        //SDK Version (set by platform-specific SDK if used)
        private string _sdkVersion;
        public string SdkVersion
        {
            get {
                if (_sdkVersion == null)
                {
                    this._sdkVersion = SDKConstants.SdkVersion;
                    if (this.PlatformHelper != null)
                    {
                        this._sdkVersion = this.PlatformHelper.SDKVersion;
                    }
                }
                return _sdkVersion; }
            set { _sdkVersion = value; }
        }

        //App info
        private string _appIdentifier;
        public string AppIdentifier
        {
            get { return _appIdentifier; }
            set
            {
                if (!String.IsNullOrEmpty(_appIdentifier))
                {
                    throw new Exception("Repeated initialization of HockeyClient! Please make sure to call the Configure(..) method only once!");
                }
                _appIdentifier = value;
            }
        }

        //Current Version of the app as string
        private string _versionInfo;
        public string VersionInfo
        {
            get
            {
                if (_versionInfo == null && this.PlatformHelper != null)
                {
                    this._versionInfo = this.PlatformHelper.AppVersion;
                }
                return _versionInfo;
            }
            set { _versionInfo = value; }
        }
        //UserID of current user
        public string UserID { get; set; }
        //Contact information for current user
        public string ContactInformation { get; set; }
        //Operating system (set by platform-specific SDK if used)
        private string _os;
        public string Os
        {
            get
            {
                if (_os == null && this.PlatformHelper != null)
                {
                    this._os = this.PlatformHelper.OSPlatform;
                }
                return _os;
            }
            set { _os = value; }
        }

        //Operating system version (set by platform-specific SDK if used)
        private string _osVersion;
        public string OsVersion
        {
            get
            {
                if (_osVersion == null && this.PlatformHelper != null)
                {
                    this._osVersion = this.PlatformHelper.OSVersion;
                }
                return _osVersion;
            }
            set { _osVersion = value; }
        }

        //Device (set by platform-specific SDK if used)
        private string _device;
        public string Device
        {
            get
            {
                if (_device == null && this.PlatformHelper != null)
                {
                    this._device = this.PlatformHelper.Model;
                }
                return _device;
            }
            set { _device = value; }
        }

        //Oem of Device (set by platform-specific SDK if used)
        private string _oem;
        public string Oem
        {
            get
            {
                if (_oem == null && this.PlatformHelper != null)
                {
                    this._oem = this.PlatformHelper.Manufacturer;
                }
                return _oem;
            }
            set { _oem = value; }
        }

        //uniques user id provided by platform (set by platform-specific SDK if used)
        public string Uuid { get; set; }
        //Authorized user id (set during login process)
        public string Auid { get; internal set; }
        //Identified user id (set during login process)
        public string Iuid { get; internal set; }

        //Delegate which can be set to add a description to a stacktrace when app crashes
        public Func<Exception, string> DescriptionLoader { get; set; }

        #endregion

        #region ctor
        private ILog _logger = HockeyLogManager.GetLog(typeof(HockeyClient));
        private static HockeyClient _instance = null;

        /// <summary>
        /// Configures the HockeyClient with your app specific information
        /// </summary>
        /// <param name="appIdentifier">public identfier of your app (AppId)</param>
        /// <param name="versionInfo">version of your app</param>
        /// <param name="apiBase">[optional] the base url of the hockeyapp server. Only needed if used with a private HockeyApp installation.</param>
        /// <param name="userID">[optional] ID of the current user using your app, sent with crash-reports, can also be set via property.</param>
        /// <param name="contactInformation">[optional] contact info of the current user using your app, sent with crash-reports, can also be set via property.</param>
        [Obsolete("Use HockeyClient.Current.Configure(...)")]
        public static void Configure(string appIdentifier,
                                        string versionInfo,
                                        string apiBase = null,
                                        string userID = null,
                                        string contactInformation = null,
                                        Func<Exception, string> descriptionLoader = null)
        {
#pragma warning disable 618 // disable obsolete warning!
            ConfigureInternal(appIdentifier, versionInfo, apiBase, userID, contactInformation, null, null, null, descriptionLoader);
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
        [Obsolete("Use HockeyClient.Current.Configure(...)")]
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
            _instance.DescriptionLoader = descriptionLoader;
#pragma warning disable 618 // disable obsolete warning!
            _instance.ApiBase = apiBase ?? SDKConstants.PublicApiBase;
#pragma warning restore 618
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
        [Obsolete("Use the more idiomatic Method IHockeyClient.Current")]
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

        /// <summary>
        /// The current singleton instance of HockeyClient. Use the extension methods in the HockeyApp namespace 
        /// to work with the instance:
        /// HockeyClient.Current.Configure(..) must be called first to initialize the client!
        /// </summary>
        public static IHockeyClient Current
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new HockeyClient();
                }
                return _instance;
            }
        }

        private HockeyClient() { }

        #endregion

        public void CheckForInitialization()
        {
            if (String.IsNullOrEmpty(_appIdentifier))
            {
                throw new Exception("HockeyClient not initialized! Please make sure to call the Configure(..) method first!");
            }
        }

        #region API functions

        #region Crashes

        public ICrashData CreateCrashData(Exception ex)
        {
            return new CrashData(this, ex, this.PrefilledCrashLogInfo);
        }

        public ICrashData CreateCrashData(Exception ex, CrashLogInformation crashLogInfo)
        {
            return new CrashData(this, ex, crashLogInfo);
        }

        public ICrashData Deserialize(Stream inputStream)
        {
            return CrashData.Deserialize(inputStream);
        }

        public async Task<IEnumerable<string>> GetCrashFileNamesAsync()
        {
            return await this.PlatformHelper.GetFileNamesAsync(SDKConstants.CrashDirectoryName, SDKConstants.CrashFilePrefix + "*.log");
        }

        public async Task DeleteAllCrashesAsync()
        {
            foreach (string filename in await this.GetCrashFileNamesAsync())
            {
                try
                {
                    await this.PlatformHelper.DeleteFileAsync(filename, SDKConstants.CrashDirectoryName);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            }
        }

        public async Task<bool> AnyCrashesAvailableAsync() { return (await GetCrashFileNamesAsync()).Any(); }

        public async Task HandleExceptionAsync(Exception ex)
        {
            ICrashData cd = this.CreateCrashData(ex);

            var crashId = Guid.NewGuid();
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    cd.Serialize(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    await this.PlatformHelper.WriteStreamToFileAsync(stream, string.Format("{0}{1}.log", SDKConstants.CrashFilePrefix, crashId), SDKConstants.CrashDirectoryName);
                }
            }
            catch
            {
                // Ignore all exceptions
            }
        }

        public async Task<bool> SendCrashesAndDeleteAfterwardsAsync()
        {
            bool atLeatOneCrashSent = false;
            //System Semaphore would be another possibility. But the worst thing that can happen now, is
            //that a crash is send twice.
            if (!Monitor.TryEnter(this))
            {
                logger.Warn("Sending crashes was called multiple times!");
                throw new Exception("Hockey is already sending crashes to server!");
            }
            else
            {
                logger.Info("Start send crashes to platform.");

                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    try
                    {
                        foreach (string filename in await this.GetCrashFileNamesAsync())
                        {
                            logger.Info("Crashfile found: {0}", filename);
                            try
                            {
                                using (var stream = await this.PlatformHelper.GetStreamAsync(filename, SDKConstants.CrashDirectoryName))
                                {
                                    ICrashData cd = this.Deserialize(stream);
                                    await cd.SendDataAsync();
                                }
                                await this.PlatformHelper.DeleteFileAsync(filename, SDKConstants.CrashDirectoryName);
                                atLeatOneCrashSent = true;
                            }
                            catch (Exception ex)
                            {
                                logger.Error(ex);
                            }
                        }
                    }
                    catch (WebTransferException) { }
                    catch (Exception e) { this.logger.Error(e); }
                }
            }
            Monitor.Exit(this);
            return atLeatOneCrashSent;
        }


        #endregion

        #region Update

        public async Task<IEnumerable<IAppVersion>> GetAppVersionsAsync()
        {
            StringBuilder url = new StringBuilder(this.ApiBaseVersion2 + "apps/" + this.AppIdentifier + ".json");

            url.Append("?app_version=" + Uri.EscapeDataString(this.VersionInfo));
            if (!String.IsNullOrEmpty(this.Os)) { url.Append("&os=" + Uri.EscapeDataString(this.Os)); }
            if (!String.IsNullOrEmpty(this.OsVersion)) { url.Append("&os_version=" + Uri.EscapeDataString(this.OsVersion)); }
            if (!String.IsNullOrEmpty(this.Device)) { url.Append("&device=" + Uri.EscapeDataString(this.Device)); }
            if (!String.IsNullOrEmpty(this.Oem)) { url.Append("&oem=" + Uri.EscapeDataString(this.Oem)); }
            if (!String.IsNullOrEmpty(this.SdkName)) { url.Append("&sdk=" + Uri.EscapeDataString(this.SdkName)); }
            if (!String.IsNullOrEmpty(this.SdkVersion)) { url.Append("&sdk_version=" + Uri.EscapeDataString(this.SdkVersion)); }
            url.Append("&lang=" + Uri.EscapeDataString(System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName));

            if (!String.IsNullOrEmpty(this.Auid)) { url.Append("&=auid" + Uri.EscapeDataString(this.Auid)); }
            else if (!String.IsNullOrEmpty(this.Iuid)) { url.Append("&=iuid" + Uri.EscapeDataString(this.Iuid)); }
            else if (!String.IsNullOrEmpty(this.Uuid)) { url.Append("&=duid" + Uri.EscapeDataString(this.Uuid)); }

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

        public async Task<IAuthStatus> AuthorizeUserAsync(string email, string password)
        {
            var request = WebRequest.CreateHttp(new Uri(this.ApiBaseVersion3 + "apps/" +
                                                           this.AppIdentifier + "/identity/authorize", UriKind.Absolute));

            byte[] credentialBuffer = new UTF8Encoding().GetBytes(email + ":" + password);
            request.SetHeader(HttpRequestHeader.Authorization.ToString(), "Basic " + Convert.ToBase64String(credentialBuffer));
            request.SetHeader(HttpRequestHeader.UserAgent.ToString(), this.UserAgentString);
            request.Method = "POST";
            var status = await AuthStatus.DoAuthRequestHandleResponseAsync(request);
            if (status.IsAuthorized)
            {
                this.Auid = (status as AuthStatus).Auid;
                this.FillEmptyUserAndContactInfo(email);
            }
            return status;
        }

        public async Task<IAuthStatus> IdentifyUserAsync(string email, string appSecret)
        {
            var request = WebRequest.CreateHttp(new Uri(this.ApiBaseVersion3 + "apps/" +
                                                           this.AppIdentifier + "/identity/check", UriKind.Absolute));
            request.SetHeader(HttpRequestHeader.UserAgent.ToString(), this.UserAgentString);
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
            var status = await AuthStatus.DoAuthRequestHandleResponseAsync(request);
            if (status.IsIdentified)
            {
                this.Iuid = (status as AuthStatus).Iuid;
                this.FillEmptyUserAndContactInfo(email);
            }
            return status;
        }

        #endregion
        #endregion

        #region PlatformHelper

        public IHockeyPlatformHelper PlatformHelper { get; set; }

        CrashLogInformation? _crashLogInfo = null;
        public CrashLogInformation PrefilledCrashLogInfo
        {
            get
            {
                if (!_crashLogInfo.HasValue)
                {
                    this.CheckForInitialization();
                    if (PlatformHelper == null) { throw new Exception("HockeyClient PlatformHelper is null!"); }
                    _crashLogInfo = new CrashLogInformation()
                    {
                        PackageName = this.PlatformHelper.AppPackageName,
                        OperatingSystem = this.PlatformHelper.OSPlatform,
                        Windows = this.PlatformHelper.GetWindowsVersionString(),
                        WindowsPhone = this.PlatformHelper.GetWindowsPhoneVersionString(),
                        Manufacturer = this.PlatformHelper.Manufacturer,
                        Model = this.PlatformHelper.Model,
                        ProductID = this.PlatformHelper.ProductID,
                        Version = this.PlatformHelper.AppVersion
                    };
                }
                return _crashLogInfo.Value;
            }
        }

        #endregion


    }
}