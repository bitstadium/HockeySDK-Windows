namespace Microsoft.HockeyApp
{
    using Microsoft.HockeyApp.Internal;
    using Services;
    using System;
    using System.Resources;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;

    /// <summary>
    /// Static extension class containing the main extension methods for controlling the HockeySDK client
    /// </summary>
    public static class HockeyClientWP8SLExtension
    {
        internal static Action<Exception> customUnobservedTaskExceptionAction;

        #region Configuration
        /// <summary>
        /// main configuration method. call in app constructor
        /// </summary>
        /// <param name="this"></param>
        /// <param name="appId"></param>
        /// <param name="rootFrame"></param>
        /// <returns></returns>
        public static IHockeyClientConfigurable Configure(this IHockeyClient @this, string appId, TelemetryConfiguration telemetryConfiguration = null, Frame rootFrame = null)
        {
            if (@this.AsInternal().TestAndSetIsConfigured())
            {
                return @this as IHockeyClientConfigurable;
            }

            @this.AsInternal().PlatformHelper = new HockeyPlatformHelperWP8SL();
            @this.AsInternal().AppIdentifier = appId;
            CrashHandler.Current.Application = Application.Current;
                
            ServiceLocator.AddService<BaseStorageService>(new StorageService());
            ServiceLocator.AddService<Services.IApplicationService>(new ApplicationService());
            ServiceLocator.AddService<Services.IPlatformService>(new PlatformService());
            ServiceLocator.AddService<IDeviceService>(new DeviceService());
            var exceptionModule = new UnhandledExceptionTelemetryModule(rootFrame);

            // we need to initialize in Configure method and not in WindowsAppInitializer.InitializeAsync 
            // to prevent UnauthorizedAccessException with Invalid cross-thread access message
            exceptionModule.Initialize();
            ServiceLocator.AddService<IUnhandledExceptionTelemetryModule>(exceptionModule);
            WindowsAppInitializer.InitializeAsync(appId, telemetryConfiguration);
            return @this as IHockeyClientConfigurable;
        }

        /// <summary>
        /// Provide a custom resource manager to override standard sdk i18n strings
        /// </summary>
        /// <param name="this"></param>
        /// <param name="manager">resource manager to use</param>
        /// <returns></returns>
        public static IHockeyClientConfigurable UseCustomResourceManager(this IHockeyClientConfigurable @this, ResourceManager manager)
        {
            //TODO make LocalizedStrings.CustomResourceManager internal in next major version
            #pragma warning disable 0618
            LocalizedStrings.CustomResourceManager = manager;
            #pragma warning restore 0618
            return @this;
        }

        #endregion

        #region Wrappers for functions

        #region CrashHandling

        /// <summary>
        /// Send any collected crashes to the HockeyApp server. You should normally call this during startup of your app. 
        /// </summary>
        /// <param name="this"></param>
        /// <param name="sendWithoutAsking">configures if available crashes are sent immediately or if the user should be asked if the crashes should be sent or discarded</param>
        /// <returns>true if crashes where sent successfully</returns>
        public static async Task<bool> SendCrashesAsync(this IHockeyClient @this, bool sendWithoutAsking = false)
        {
            @this.AsInternal().CheckForInitialization();
            return await CrashHandler.Current.HandleCrashesAsync(sendWithoutAsking).ConfigureAwait(false);
        }
        
        /// <summary>
        /// The action you set will be called after HockeyApp has written the crash-log and allows you to run custom logic like marking the exception as handled
        /// </summary>
        /// <param name="customAction">The custom action.</param>
        /// <returns></returns>
        public static IHockeyClientConfigurable RegisterCustomUnhandledExceptionLogic(this IHockeyClientConfigurable @this, Action<ApplicationUnhandledExceptionEventArgs> customAction)
        {
            UnhandledExceptionTelemetryModule.CustomUnhandledExceptionAction = customAction;
            return @this;
        }

        /// <summary>
        /// The action you set will be called after HockeyApp has written the crash-log and allows you to run custom logic like exiting the application
        /// </summary>
        /// <param name="customFunc">The custom action.</param>
        /// <returns></returns>
        public static IHockeyClientConfigurable RegisterCustomUnobserveredTaskExceptionLogic(this IHockeyClientConfigurable @this, Action<Exception> customAction)
        {
            customUnobservedTaskExceptionAction = customAction;
            return @this;
        }

        #endregion

        #region Authentication

        /// <summary>
        /// Authorizes user with hockeyapp auth. Opening a login page to require (hockeaypp) userid and pass from user if needed
        /// </summary>
        /// <param name="this"></param>
        /// <param name="successRedirect">Page-URI to redirect to after successful login</param>
        /// <param name="navigationService">[optional] obsolete - not needed</param>
        /// <param name="eMail">[Optional] initial email</param>
        /// <param name="tokenValidationPolicy"><see cref="TokenValidationPolicy"/></param>
        /// <param name="authValidationMode"><see cref="AuthValidationMode"/></param>
        public static void AuthorizeUser(this IHockeyClient @this, 
            Uri successRedirect, NavigationService navigationService = null,
            string eMail = null,
            TokenValidationPolicy tokenValidationPolicy = TokenValidationPolicy.EveryLogin,
            AuthValidationMode authValidationMode = AuthValidationMode.Graceful)

        {
            @this.AsInternal().CheckForInitialization();
            AuthManager.Instance.AuthenticateUser(successRedirect, AuthenticationMode.Authorize, 
                tokenValidationPolicy, authValidationMode, eMail, null);
        }

        /// <summary>
        /// Identify user with hockeaypp auth. Opening a login page to require valid email address for app if needed
        /// </summary>
        /// <param name="this"></param>
        /// <param name="appSecret">Your app's app secret (see HockeyApp app page)</param>
        /// <param name="successRedirect">Page-URI to redirect to after successful login</param>
        /// <param name="navigationService">[optional] obsolete - not needed</param>
        /// <param name="eMail">[Optional] initial email</param>
        /// <param name="tokenValidationPolicy"><see cref="TokenValidationPolicy"/></param>
        /// <param name="authValidationMode"><see cref="AuthValidationMode"/></param>
        public static void IdentifyUser(this IHockeyClient @this, string appSecret,
            Uri successRedirect, NavigationService navigationService = null,
            string eMail = null,
            TokenValidationPolicy tokenValidationPolicy = TokenValidationPolicy.EveryLogin,
            AuthValidationMode authValidationMode = AuthValidationMode.Graceful)

        {
            @this.AsInternal().CheckForInitialization();
            AuthManager.Instance.AuthenticateUser(successRedirect, AuthenticationMode.Identify, 
                tokenValidationPolicy, authValidationMode, eMail, appSecret);
        }

        /// <summary>
        /// Logout the user by forgetting validation token
        /// </summary>
        /// <param name="this">The this.</param>
        public static void LogoutUser(this IHockeyClient @this)
        {
            AuthManager.Instance.RemoveUserToken();
        }

        #endregion

        #region Feedback

        /// <summary>
        /// Shows the feedback UI.
        /// </summary>
        /// <param name="this">The this.</param>
        /// <param name="navigationService">NavigationService</param>
        [Obsolete("Use ShowFeedback() instead")]
        public static void ShowFeedbackUI(this IHockeyClient @this, NavigationService navigationService)
        {
            @this.AsInternal().CheckForInitialization();
            FeedbackManager.Instance.NavigateToFeedbackUI(navigationService);
        }

        /// <summary>
        /// Open the feedback UI
        /// </summary>
        /// <param name="this"></param>
        public static void ShowFeedback(this IHockeyClient @this)
        {
            @this.AsInternal().CheckForInitialization();
            FeedbackManager.Instance.NavigateToFeedbackUI();
        }

        #endregion

        #region Updates

        /// <summary>
        /// Check for available updates
        /// </summary>
        /// <param name="this"></param>
        /// <param name="settings"><see cref="UpdateCheckSettings"/></param>
        public static void CheckForUpdates(this IHockeyClient @this, UpdateCheckSettings settings = null)
        {
            @this.AsInternal().CheckForInitialization();
            UpdateManager.Instance.RunUpdateCheck(settings);
        }

        #endregion

        #endregion
    }
}
