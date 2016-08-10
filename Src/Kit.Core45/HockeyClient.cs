namespace Microsoft.HockeyApp
{
    using Channel;
    using DataContracts;
    using Exceptions;
    using Extensibility;
    using Extensibility.Implementation;
    using Extensibility.Implementation.Tracing;
    using Extensions;
    using Internal;
    using Model;
    using Services;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Implements the HockeyClient singleton
    /// </summary>
    public class HockeyClient : HockeyApp.IHockeyClient, IHockeyClientInternal, IHockeyClientConfigurable
    {
        private const int MaxQueueSize = 4096;

        private static HockeyClient _instance = null;

        /// <summary>
        /// Telemetry buffer of items that were tracked by the user before configuration has been initialized.
        /// </summary>
        private readonly Queue<ITelemetry> queue = new Queue<ITelemetry>();
        private ILog logger = HockeyLogManager.GetLog(typeof(HockeyClient));
        private TelemetryContext context;
        private ITelemetryChannel channel;

        private HockeyClient()
        {
        }

        #region Properties

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

        /// <summary>
        /// ApiBase of HockeyApp server
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", MessageId = "System.String.EndsWith(System.String)")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", MessageId = "System.String.IndexOf(System.String)", Justification = "ToDo: Perform refactoring after HA/AI SDK integration.")]
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
                        domain = value.Substring(0, value.IndexOf("/api/", StringComparison.OrdinalIgnoreCase) + 1);
                    }
                    ApiDomain = domain.EndsWith("/", StringComparison.OrdinalIgnoreCase) ? domain : domain + "/";
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", MessageId = "System.String.EndsWith(System.String)")]
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
            get
            {
                if (_userAgentString == null)
                {
                    this._userAgentString = SDKConstants.UserAgentString;
                    if (this.PlatformHelper != null)
                    {
                        this._userAgentString = this.PlatformHelper.UserAgentString;
                    }
                }
                return _userAgentString;
            }
            set { _userAgentString = value; }
        }


        private string _sdkName;
        /// <summary>
        /// SDK info
        /// </summary>
        public string SdkName
        {
            get
            {
                if (_sdkName == null)
                {
                    this._sdkName = SDKConstants.SdkName;
                    if (this.PlatformHelper != null)
                    {
                        this._sdkName = this.PlatformHelper.SDKName;
                    }
                }
                return _sdkName;
            }
            set { _sdkName = value; }
        }

        private string _sdkVersion;
        /// <summary>
        /// SDK Version
        /// </summary>
        public string SdkVersion
        {
            get
            {
                if (_sdkVersion == null)
                {
                    this._sdkVersion = SDKConstants.SdkVersion;
                    if (this.PlatformHelper != null)
                    {
                        this._sdkVersion = this.PlatformHelper.SDKVersion;
                    }
                }
                return _sdkVersion;
            }
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
                // in TelemetryClient the OS version is correctly identified, use it if TelemetryClient initialized.
                // otherwise go to PlatformHelper implementation.
                if (IsTelemetryInitialized && Context != null && Context.Device != null && Context.Device != null)
                {
                    return this.Context.Device.DeviceOSVersion;
                }

                if (_osVersion == null && this.PlatformHelper != null)
                {
                    this._osVersion = this.PlatformHelper.OSVersion;
                }

                return _osVersion;
            }
            set
            {
                _osVersion = value;
            }
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

        /// <summary>
        /// Gets a value indicating whether telemetry client has been initialized.
        /// </summary>
        public bool IsTelemetryInitialized
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the current context that will be used to augment telemetry you send.
        /// </summary>
        internal TelemetryContext Context
        {
            get
            {
                // In order to prevent a deadlock, we are calling async method from sync using Task.Run to offload a work to a ThreadPool
                // thread which does not have a SynchronizationContext and there is no real risk for a deadlock.
                // http://stackoverflow.com/questions/28305968/use-task-run-in-synchronous-method-to-avoid-deadlock-waiting-on-async-method
                LazyInitializer.EnsureInitialized(ref this.context, () => { return Task.Run(async () => { return await this.CreateInitializedContextAsync(); }).Result; });
                return this.context;
            }

            set
            {
                this.context = value;
            }
        }

        /// <summary>
        /// Gets or sets the default instrumentation key for all <see cref="ITelemetry"/> objects logged.
        /// </summary>
        internal string InstrumentationKey
        {
            get { return this.Context.InstrumentationKey; }
            set { this.Context.InstrumentationKey = value; }
        }

        /// <summary>
        /// Gets or sets the channel used by the client helper. Note that this doesn't need to be public as a customer can create a new client 
        /// with a new channel via telemetry configuration.
        /// </summary>
        internal ITelemetryChannel Channel
        {
            get
            {
                ITelemetryChannel output = this.channel;
                if (output == null)
                {
                    output = TelemetryConfiguration.Active.TelemetryChannel;
                    this.channel = output;
                }

                return output;
            }

            set
            {
                this.channel = value;
            }
        }

        #endregion


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

        #region Events
        
        /// <summary>
        /// Subscribe to this event to get all exceptions that are swallowed by HockeySDK.
        /// Useful for debugging. Be sure to know what to do if you use this in production code.
        /// </summary>
        public event EventHandler<InternalUnhandledExceptionEventArgs> OnHockeySDKInternalException;


        #endregion

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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object[])")]
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

        #region PlatformHelper

        /// <summary>
        /// Platform helper for internal use
        /// </summary>
        public IHockeyPlatformHelper PlatformHelper { get; set; }

        CrashLogInformation? _crashLogInfo = null;
        /// <summary>
        /// A filled CrashLogInformation object
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
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
                        Windows = OsVersion,
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

        /// <summary>
        /// Handle Exceptions that are swallowed because we don't want our SDK crash other apps
        /// For internal use by platform SDKs
        /// </summary>
        /// <param name="unhandledException">the exception to propagate</param>
        public void HandleInternalUnhandledException(Exception unhandledException)
        {
            logger.Error(unhandledException);
            var args = new InternalUnhandledExceptionEventArgs() { Exception = unhandledException };
            OnHockeySDKInternalException?.Invoke(this, args);
        }

        /// <summary>
        /// Send a custom event for display in Events tab.
        /// </summary>
        /// <param name="eventName">Event name</param>
        public void TrackEvent(string eventName)
        {
            this.TrackEvent(new EventTelemetry(eventName));
        }

        /// <summary>
        /// Send an event telemetry for display in Diagnostic Search and aggregation in Metrics Explorer.
        /// </summary>
        /// <param name="eventName">A name for the event.</param>
        /// <param name="properties">Named string values you can use to search and classify events.</param>
        /// <param name="metrics">Measurements associated with this event.</param>
        public void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            var telemetry = new EventTelemetry(eventName);

            if (properties != null && properties.Count > 0)
            {
                Utils.CopyDictionary(properties, telemetry.Context.Properties);
            }

            if (metrics != null && metrics.Count > 0)
            {
                Utils.CopyDictionary(metrics, telemetry.Metrics);
            }

            this.TrackEvent(telemetry);
        }

        /// <summary>
        /// Send a trace message for display in Diagnostic Search.
        /// </summary>
        /// <param name="message">Message to display.</param>
        public void TrackTrace(string message)
        {
            this.TrackTrace(new TraceTelemetry(message));
        }

        /// <summary>
        /// Send a trace message for display in Diagnostic Search.
        /// </summary>
        /// <param name="message">Message to display.</param>
        /// <param name="severityLevel">Trace severity level.</param>
        public void TrackTrace(string message, SeverityLevel severityLevel)
        {
            this.TrackTrace(new TraceTelemetry(message, severityLevel));
        }

        /// <summary>
        /// Send a trace message for display in Diagnostic Search.
        /// </summary>
        /// <param name="message">Message to display.</param>
        /// <param name="properties">Named string values you can use to search and classify events.</param>
        public void TrackTrace(string message, IDictionary<string, string> properties)
        {
            TraceTelemetry telemetry = new TraceTelemetry(message);

            if (properties != null && properties.Count > 0)
            {
                Utils.CopyDictionary(properties, telemetry.Context.Properties);
            }

            this.TrackTrace(telemetry);
        }

        /// <summary>
        /// Send a trace message for display in Diagnostic Search.
        /// </summary>
        /// <param name="message">Message to display.</param>
        /// <param name="severityLevel">Trace severity level.</param>
        /// <param name="properties">Named string values you can use to search and classify events.</param>
        public void TrackTrace(string message, SeverityLevel severityLevel, IDictionary<string, string> properties)
        {
            TraceTelemetry telemetry = new TraceTelemetry(message, severityLevel);

            if (properties != null && properties.Count > 0)
            {
                Utils.CopyDictionary(properties, telemetry.Context.Properties);
            }

            this.TrackTrace(telemetry);
        }

        /// <summary>
        /// Send a trace message for display in Diagnostic Search.
        /// </summary>
        /// <param name="telemetry">Message with optional properties.</param>
        public void TrackTrace(TraceTelemetry telemetry)
        {
            telemetry = telemetry ?? new TraceTelemetry();
            this.Track(telemetry);
        }

        /// <summary>
        /// Send a <see cref="MetricTelemetry"/> for aggregation in Metric Explorer.
        /// </summary>
        /// <param name="name">Metric name.</param>
        /// <param name="value">Metric value.</param>
        /// <param name="properties">Named string values you can use to classify and filter metrics.</param>
        public void TrackMetric(string name, double value, IDictionary<string, string> properties = null)
        {
            var telemetry = new MetricTelemetry(name, value);
            if (properties != null && properties.Count > 0)
            {
                Utils.CopyDictionary(properties, telemetry.Properties);
            }

            this.TrackMetric(telemetry);
        }

        /// <summary>
        /// Send information about the page viewed in the application.
        /// </summary>
        /// <param name="name">Name of the page.</param>
        public void TrackPageView(string name)
        {
            this.Track(new PageViewTelemetry(name));
        }

        /// <summary>
        /// Send a <see cref="MetricTelemetry"/> for aggregation in Metric Explorer.
        /// </summary>
        public void TrackMetric(MetricTelemetry telemetry)
        {
            if (telemetry == null)
            {
                telemetry = new MetricTelemetry();
            }

            this.Track(telemetry);
        }

        /// <summary>
        /// Send an <see cref="ExceptionTelemetry"/> for display in Diagnostic Search.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="properties">Named string values you can use to classify and search for this exception.</param>
        public void TrackException(Exception exception, IDictionary<string, string> properties = null)
        {
            if (exception == null)
            {
                exception = new Exception(Utils.PopulateRequiredStringValue(null, "message", typeof(ExceptionTelemetry).FullName));
            }

            var service = ServiceLocator.GetService<IUnhandledExceptionTelemetryModule>();

            // if service is not implemented, use default exception telemetry
            ITelemetry telemetry = service == null ? telemetry = new ExceptionTelemetry(exception) { HandledAt = ExceptionHandledAt.UserCode } : service.CreateCrashTelemetry(exception, ExceptionHandledAt.UserCode);
            if (properties != null && properties.Count > 0)
            {
                Utils.CopyDictionary(properties, telemetry.Context.Properties);
            }

            this.Track(telemetry);
        }

        /// <summary>
        /// Send an <see cref="ExceptionTelemetry"/> for display in Diagnostic Search.
        /// </summary>
        internal void TrackException(ExceptionTelemetry telemetry)
        {
            if (telemetry == null)
            {
                var exception = new Exception(Utils.PopulateRequiredStringValue(null, "message", typeof(ExceptionTelemetry).FullName));
                telemetry = new ExceptionTelemetry(exception)
                {
                    HandledAt = ExceptionHandledAt.UserCode,
                };
            }

            this.Track(telemetry);
        }

        /// <summary>
        /// Send information about external dependency call in the application.
        /// </summary>
        /// <param name="dependencyName">External dependency name.</param>
        /// <param name="commandName">Dependency call command name.</param>
        /// <param name="startTime">The time when the dependency was called.</param>
        /// <param name="duration">The time taken by the external dependency to handle the call.</param>
        /// <param name="success">True if the dependency call was handled successfully.</param>
        public void TrackDependency(string dependencyName, string commandName, DateTimeOffset startTime, TimeSpan duration, bool success)
        {
            this.TrackDependency(new DependencyTelemetry(dependencyName, commandName, startTime, duration, success));
        }

        /// <summary>
        /// Send information about external dependency call in the application.
        /// </summary>
        public void TrackDependency(DependencyTelemetry telemetry)
        {
            if (telemetry == null)
            {
                telemetry = new DependencyTelemetry();
            }

            this.Track(telemetry);
        }

        /// <summary>
        /// This method is an internal part of Application Insights infrastructure. Do not call.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal void Track(ITelemetry telemetry)
        {
            if (!IsTelemetryInitialized)
            {
                CoreEventSource.Log.LogVerbose("HockeyClient configuration has not been initialized. Saving telemetry item to a queue.");
                queue.Enqueue(telemetry);
                if (queue.Count > MaxQueueSize)
                {
                    queue.Dequeue();
                }

                return;
            }

            // TALK TO YOUR TEAM MATES BEFORE CHANGING THIS.
            // This method needs to be public so that we can build and ship new telemetry types without having to ship core.
            // It is hidden from intellisense to prevent customer confusion.
            if (this.IsTelemetryEnabled())
            {
                string instrumentationKey = this.Context.InstrumentationKey;

                if (string.IsNullOrEmpty(instrumentationKey))
                {
                    instrumentationKey = TelemetryConfiguration.Active.InstrumentationKey;
                }

                if (string.IsNullOrEmpty(instrumentationKey))
                {
                    return;
                }

                var telemetryWithProperties = telemetry as ISupportProperties;
                if (telemetryWithProperties != null)
                {
                    if (this.Channel.DeveloperMode.HasValue && this.Channel.DeveloperMode.Value)
                    {
                        if (!telemetryWithProperties.Properties.ContainsKey("DeveloperMode"))
                        {
                            telemetryWithProperties.Properties.Add("DeveloperMode", "true");
                        }
                    }

                    Utils.CopyDictionary(this.Context.Properties, telemetryWithProperties.Properties);
                }

                telemetry.Context.Initialize(this.Context, instrumentationKey);
                foreach (ITelemetryInitializer initializer in TelemetryConfiguration.Active.TelemetryInitializers)
                {
                    try
                    {
                        initializer.Initialize(telemetry);
                    }
                    catch (Exception exception)
                    {
                        CoreEventSource.Log.LogError(string.Format(
                                                        CultureInfo.InvariantCulture,
                                                        "Exception while initializing {0}, exception message - {1}",
                                                        initializer.GetType().FullName,
                                                        exception.ToString()));
                    }
                }

                telemetry.Sanitize();
                this.Channel.Send(telemetry);
                this.WriteTelemetryToDebugOutput(telemetry);
            }
        }


        /// <summary>
        /// Send information about the page viewed in the application.
        /// </summary>
        public void TrackPageView(PageViewTelemetry telemetry)
        {
            if (telemetry == null)
            {
                telemetry = new PageViewTelemetry();
            }

            this.Track(telemetry);
        }

        /// <summary>
        /// Send information about a request handled by the application.
        /// </summary>
        /// <param name="name">The request name.</param>
        /// <param name="startTime">The time when the page was requested.</param>
        /// <param name="duration">The time taken by the application to handle the request.</param>
        /// <param name="responseCode">The response status code.</param>
        /// <param name="success">True if the request was handled successfully by the application.</param>
        internal void TrackRequest(string name, DateTimeOffset startTime, TimeSpan duration, string responseCode, bool success)
        {
            this.Track(new RequestTelemetry(name, startTime, duration, responseCode, success));
        }

        /// <summary>
        /// Send information about a request handled by the application.
        /// </summary>
        internal void TrackRequest(RequestTelemetry request)
        {
            if (request == null)
            {
                request = new RequestTelemetry();
            }

            this.Track(request);
        }

        /// <summary>
        /// Send an <see cref="EventTelemetry"/> for display in Diagnostic Search and aggregation in Metrics Explorer.
        /// </summary>
        /// <param name="telemetry">An event log item.</param>
        public void TrackEvent(EventTelemetry telemetry)
        {
            if (telemetry != null)
            {
                this.Track(telemetry);
            }
        }

        /// <summary>
        /// Initializes telemetry client.
        /// For performance reasons, this call needs to be performed only after <see cref="TelemetryConfiguration"/> has been initialized.
        /// </summary>
        internal void Initialize()
        {
            this.IsTelemetryInitialized = true;
            while (queue.Count > 0)
            {
                this.Track(queue.Dequeue());
            }
        }

        /// <summary>
        /// Check to determine if the tracking is enabled.
        /// </summary>
        internal bool IsTelemetryEnabled()
        {
            return !TelemetryConfiguration.Active.DisableTelemetry;
        }

        /// <summary>
        /// Clears all buffers for this telemetry stream and causes any buffered data to be written to the underlying channel.
        /// </summary>
        public void Flush()
        {
            this.Channel.Flush();
        }

        /// <summary>
        /// Clears all buffers for this telemetry stream and causes any buffered data to be written to the underlying channel.
        /// And send all persistent telemetry data, if persistence channel is used and if possible with current conditions otherwise telemetry data still persist.
        /// </summary>
        public void FlushAndSendPersistentTelemetry()
        {
            this.Channel.FlushAndSend();
        }

        private async Task<TelemetryContext> CreateInitializedContextAsync()
        {
            var context = new TelemetryContext();
            foreach (IContextInitializer initializer in TelemetryConfiguration.Active.ContextInitializers)
            {
                await initializer.Initialize(context);
            }

            return context;
        }

        private void WriteTelemetryToDebugOutput(ITelemetry telemetry)
        {
            if (CoreEventSource.Log.Enabled)
            {
                using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
                {
                    string serializedTelemetry = JsonSerializer.SerializeAsString(telemetry);
                    CoreEventSource.Log.LogVerbose("HockeySDK Telemetry: " + serializedTelemetry);
                }
            }
        }
    }
}