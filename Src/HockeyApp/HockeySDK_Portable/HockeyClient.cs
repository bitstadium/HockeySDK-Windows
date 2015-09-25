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

    /// <summary>
    /// Implements the HockeyClient singleton
    /// </summary>
    public class HockeyClient : HockeyApp.IHockeyClient, IHockeyClientInternal, IHockeyClientConfigurable
    {
        private ILog logger = HockeyLogManager.GetLog(typeof(HockeyClient));

        #region fields

        /// <summary>
        /// ApiBase of HockeyApp server
        /// </summary>
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

        private string _apiDomain = SDKConstants.PublicApiDomain + "/";
        /// <summary>
        /// Base URL (prototcol+domainname) of HockeyApp server
        /// </summary>
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

        /// <summary>
        /// API endpoint for API v2
        /// </summary>
        public string ApiBaseVersion2
        {
            get { return ApiDomain + "api/2/"; }
        }

        /// <summary>
        /// API endpoint for API v3
        /// </summary>
        public string ApiBaseVersion3
        {
            get { return ApiDomain + "api/3/"; }
        }

        
        private string _userAgentString;
        /// <summary>
        /// User agent string
        /// </summary>
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

        
        private string _sdkName;
        /// <summary>
        /// SDK info
        /// </summary>
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

        
        private string _sdkVersion;
        /// <summary>
        /// SDK Version
        /// </summary>
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

        private string _appIdentifier;
        /// <summary>
        /// Public identifier of your app
        /// </summary>
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

        private string _versionInfo;
        /// <summary>
        /// Version of the app as string. Normally set automatically by platform-specific SDK
        /// </summary>
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
        
        /// <summary>
        /// UserID of current app user (if provided)
        /// </summary>
        public string UserID { get; set; }
        /// <summary>
        /// Contact information for current user
        /// </summary>
        public string ContactInformation { get; set; }
        //Operating system (set by platform-specific SDK if used)
        private string _os;
        /// <summary>
        /// Name of platform OS
        /// </summary>
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

        
        private string _osVersion;
        /// <summary>
        /// Operating system version (set by platform-specific SDK if used)
        /// </summary>
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

        private string _device;
        /// <summary>
        /// Device (set by platform-specific SDK if used)
        /// </summary>
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

        private string _oem;
        /// <summary>
        /// Oem of Device (set by platform-specific SDK if used)
        /// </summary>
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

        /// <summary>
        /// unique user id provided by platform (set by platform-specific SDK if used)
        /// </summary>
        public string Uuid { get; set; }
        /// <summary>
        /// Authorized user id (set during login process)
        /// </summary>
        public string Auid { get; internal set; }
        /// <summary>
        /// Identified user id (set during login process)
        /// </summary>
        public string Iuid { get; internal set; }

        /// <summary>
        /// Delegate which can be set to add a description to a stacktrace when app crashes
        /// </summary>
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
        /// <param name="descriptionLoader">[optional] description loader func to return an additional description for the exception</param>
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
        /// <param name="descriptionLoader">[optional] </param>
        /// <param name="os">[optional] </param>
        /// <param name="osVersion">[optional] </param>
        /// <param name="device">[optional] </param>
        /// <param name="oem">[optional] </param>
        /// <param name="uuid">[optional] </param>
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
            _instance.ApiBase = apiBase ?? SDKConstants.PublicApiDomain;
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
        [Obsolete("Use IHockeyClient.Current if you utilize the new extensions method HockeyClient.Current.Configure(...)")]
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

        /// <summary>
        /// Check if this HockeyClient has already been initialized (used internally by platform SDKs)
        /// </summary>
        public void CheckForInitialization()
        {
            if (String.IsNullOrEmpty(_appIdentifier))
            {
                throw new Exception("HockeyClient not initialized! Please make sure to call the Configure(..) method first!");
            }
        }

        #endregion

        #region events
        
        /// <summary>
        /// Subscribe to this event to get all exceptions that are swallowed by HockeySDK.
        /// Useful for debugging. Be sure to know what to do if you use this in production code.
        /// </summary>
        public event EventHandler<InternalUnhandledExceptionEventArgs> OnHockeySDKInternalException;

        /// <summary>
        /// Handle Exceptions that are swallowed because we don't want our SDK crash other apps
        /// For internal use by platform SDKs
        /// </summary>
        /// <param name="unhandledException">the exception to propagate</param>
        public void HandleInternalUnhandledException(Exception unhandledException) {
            logger.Error(unhandledException);
            var args = new InternalUnhandledExceptionEventArgs() { Exception = unhandledException };
            var handler = OnHockeySDKInternalException;
            if (handler != null)
                handler(this, args);
        }

        #endregion

        #region API functions

        #region Crashes

        /// <summary>
        /// Create a CrashData object from an Exception with the default CrashLogInformation
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public ICrashData CreateCrashData(Exception ex)
        {
            return new CrashData(this, ex, this.PrefilledCrashLogInfo);
        }

        /// <summary>
        /// Create a CrashData object from an Exception and a given CrashLogInformation
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="crashLogInfo"></param>
        /// <returns></returns>
        public ICrashData CreateCrashData(Exception ex, CrashLogInformation crashLogInfo)
        {
            return new CrashData(this, ex, crashLogInfo);
        }

        /// <summary>
        /// create a CrashData object from a logString and stacktrace (used for Unity crashes)
        /// </summary>
        /// <param name="logString"></param>
        /// <param name="stackTrace"></param>
        /// <returns></returns>
        public ICrashData CreateCrashData(string logString, string stackTrace)
        {
            return new CrashData(this, logString, stackTrace, this.PrefilledCrashLogInfo);
        }
        
        /// <summary>
        /// Get an ICrashData object from crashlog-stream
        /// </summary>
        /// <param name="inputStream">stream from crashlog</param>
        /// <returns>deserialized CrashData object</returns>
        public ICrashData Deserialize(Stream inputStream)
        {
            return CrashData.Deserialize(inputStream);
        }

        /// <summary>
        /// Retrieve filenames of crashlog files from storage
        /// </summary>
        /// <returns>crashlog-filenames (only name without folder)</returns>
        public async Task<IEnumerable<string>> GetCrashFileNamesAsync()
        {
            return await this.PlatformHelper.GetFileNamesAsync(SDKConstants.CrashDirectoryName, SDKConstants.CrashFilePrefix + "*.log");
        }

        /// <summary>
        /// Delete all crash-logs from storage
        /// </summary>
        /// <returns></returns>
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
                    HandleInternalUnhandledException(ex);
                }
            }
        }

        /// <summary>
        /// Check for available crash-logs in storage
        /// </summary>
        /// <returns>true if saved crashlogs are available</returns>
        public async Task<bool> AnyCrashesAvailableAsync() { return (await GetCrashFileNamesAsync()).Any(); }

        /// <summary>
        /// Handle exception asyncronously
        /// </summary>
        /// <param name="ex">the exception that should be saved to a crashlog</param>
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
            catch (Exception e)
            {
                HandleInternalUnhandledException(e);
            }
        }
        /// <summary>
        /// Handle exception syncronously (only for platforms that support sync write to storage
        /// </summary>
        /// <param name="ex">the exception that should be saved to a crashlog</param>
        public void HandleException(Exception ex)
        {
            if (!this.PlatformHelper.PlatformSupportsSyncWrite)
            {
                throw new Exception("PlatformHelper implementation error.");
            }
            ICrashData cd = this.CreateCrashData(ex);
            var crashId = Guid.NewGuid();
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    cd.Serialize(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    this.PlatformHelper.WriteStreamToFileSync(stream, string.Format("{0}{1}.log", SDKConstants.CrashFilePrefix, crashId), SDKConstants.CrashDirectoryName);
                }
            }
            catch (Exception e)
            {
                HandleInternalUnhandledException(e);
            }
        }

#if NET_4_5
        private readonly AsyncLock lck = new AsyncLock();
#endif
        /// <summary>
        /// Send crash-logs from storage and deletes the if they could be sent
        /// </summary>
        /// <returns>true if at least one Crashlog was transmitted to the server</returns>
        public async Task<bool> SendCrashesAndDeleteAfterwardsAsync()
        {
            bool atLeatOneCrashSent = false;
#if NET_4_5
            using(var releaser = await lck.LockAsync()) { 
#else
            if (Monitor.TryEnter(this))
            {
                try
                {
#endif
                    logger.Info("Start send crashes to platform.");
                    if (NetworkInterface.GetIsNetworkAvailable())
                    {
                        foreach (string filename in await this.GetCrashFileNamesAsync())
                        {
                            logger.Info("Crashfile found: {0}", filename);
                            Exception error = null;
                            try //don't stop if one file fails
                            {
                                using (var stream = await this.PlatformHelper.GetStreamAsync(filename, SDKConstants.CrashDirectoryName))
                                {
                                    ICrashData cd = this.Deserialize(stream);
                                    await cd.SendDataAsync();
                                }

                                atLeatOneCrashSent = true;
                            }
                            catch (Exception ex)
                            {
                                HandleInternalUnhandledException(ex);
                                error = ex;
                            }
                            if (error != null && error is WebTransferException)
                            {
                                //will retry on next start
                            }
                            else
                            {
                                //either no error or the file seems corrupt => try to delete it
                                try
                                {
                                    await this.PlatformHelper.DeleteFileAsync(filename, SDKConstants.CrashDirectoryName);
                                }
                                catch (Exception ex) {
                                    HandleInternalUnhandledException(ex);
                                }
                            }
                        }
                    }
                }
#if !NET_4_5
                finally
                {
                    try
                    {
                        Monitor.Exit(this);
                    }
                    catch (Exception ex)
                    { //ignore. on next start it will try again.
                        HandleInternalUnhandledException(ex);
                    }
                }
            }
#endif
            return atLeatOneCrashSent;
        }


        #endregion

        #region Update

        /// <summary>
        /// Get available app versions from the server
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Create a feedback thread to post messages on
        /// </summary>
        /// <returns>an empty IFeedbackThread</returns>
        public IFeedbackThread CreateNewFeedbackThread()
        {
            return FeedbackThread.CreateInstance();
        }

        /// <summary>
        /// Try to open an existng Feedbackthread
        /// </summary>
        /// <param name="threadToken">thread token for this thread</param>
        /// <returns>a populated feedback thread, null if the token is invalid or the thread closed.</returns>
        public async Task<IFeedbackThread> OpenFeedbackThreadAsync(string threadToken)
        {
            if (String.IsNullOrWhiteSpace(threadToken))
            {
                throw new ArgumentException("Token must not be empty!");
            }
            FeedbackThread fbThread = null;
            try
            {
                fbThread = await FeedbackThread.OpenFeedbackThreadAsync(this, threadToken);
            }
            catch (Exception e)
            {
                HandleInternalUnhandledException(e);
            }
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

        /// <summary>
        /// try to authorize a (hockeayapp) user by email and password
        /// </summary>
        /// <param name="email">email (hockeyapp user id)</param>
        /// <param name="password">password of the user</param>
        /// <returns>IAuthStatus. If successfull will contain authid and IsAuthorized will be true</returns>
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

        /// <summary>
        /// Identify a user by his email-adress (hockeyapp id)
        /// </summary>
        /// <param name="email">email (hockeyapp user id)</param>
        /// <param name="appSecret">app secret of the app</param>
        /// <returns>IAuthStatus. If sucessful (hockeyapp user exists) IsIdentified is true.</returns>
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
            IAuthStatus status;
            using (Stream stream = await request.GetRequestStreamAsync())
            {
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
                stream.Flush(); 
            }
                status = await AuthStatus.DoAuthRequestHandleResponseAsync(request);
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

        /// <summary>
        /// Platform helper for internal use
        /// </summary>
        public IHockeyPlatformHelper PlatformHelper { get; set; }

        CrashLogInformation? _crashLogInfo = null;
        /// <summary>
        /// A filled CrashLogInformation object
        /// </summary>
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