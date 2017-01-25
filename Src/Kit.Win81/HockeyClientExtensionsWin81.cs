namespace Microsoft.HockeyApp
{
    using Services;
    using Services.Device;
    using System;
    using System.Threading.Tasks;
    using Tools;
    using ViewModels;
    using Views;
    using Windows.Storage;
    using Windows.UI.Xaml;

    public static class HockeyClientExtensionsWin81
    {
        private static Func<UnobservedTaskExceptionEventArgs, bool> customUnobservedTaskExceptionFunc;

        #region Configure
        /// <summary>
        /// This is the main configuration method. Call this in the Constructor of your app. This registers an error handler for unhandled errors.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="appIdentifier">Your unique app id from HockeyApp.</param>
        /// <param name="endpointAddress">The HTTP address where the telemetry is sent.</param>
        /// <param name="configuration">Telemetry configuration.</param>
        /// <returns>Configurable Hockey client. Configure additional settings by calling methods on the returned IHockeyClientConfigurable</returns>
        public static IHockeyClientConfigurable Configure(this IHockeyClient @this, string appIdentifier, TelemetryConfiguration configuration = null)
        {
            if (@this.AsInternal().TestAndSetIsConfigured())
            {
                return @this as IHockeyClientConfigurable;
            }

            @this.AsInternal().PlatformHelper = new HockeyPlatformHelper81();
            @this.AsInternal().AppIdentifier = appIdentifier;

            ServiceLocator.AddService<BaseStorageService>(new StorageService());
            ServiceLocator.AddService<IApplicationService>(new ApplicationService());
            ServiceLocator.AddService<IDeviceService>(new DeviceService());
            ServiceLocator.AddService<Services.IPlatformService>(new PlatformService());
            ServiceLocator.AddService<IUnhandledExceptionTelemetryModule>(new UnhandledExceptionTelemetryModule());
            WindowsAppInitializer.InitializeAsync(appIdentifier, configuration);
            return @this as IHockeyClientConfigurable;
        }

        /// <summary>
        /// Adds the handler for UnobservedTaskException
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static IHockeyClientConfigurable RegisterUnobservedTaskExceptionHandler(this IHockeyClientConfigurable @this)
        {
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            return @this;
        }

        /// <summary>
        /// Removes the handler for UnobservedTaskException
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static IHockeyClientConfigurable UnregisterUnobservedTaskExceptionHandler(this IHockeyClientConfigurable @this)
        {
            TaskScheduler.UnobservedTaskException -= TaskScheduler_UnobservedTaskException;
            return @this;
        }

        /// <summary>
        /// The func you set will be called after HockeyApp has written the crash-log and allows you to continue
        /// If the func returns false the app will not terminate but keep running
        /// </summary>
        /// <param name="this"></param>
        /// <param name="customFunc"></param>
        /// <returns></returns>
        public static IHockeyClientConfigurable RegisterCustomUnhandledExceptionLogic(this IHockeyClientConfigurable @this, Func<UnhandledExceptionEventArgs, bool> customFunc)
        {
            UnhandledExceptionTelemetryModule.CustomUnhandledExceptionFunc = customFunc;
            return @this;
        }

        /// <summary>
        /// The func you set will be called after HockeyApp has written the crash-log and allows you to continue
        /// If the func returns false the app will not terminate but keep running
        /// </summary>
        /// <param name="this"></param>
        /// <param name="customFunc"></param>
        /// <returns></returns>
        public static IHockeyClientConfigurable RegisterCustomUnobserveredTaskExceptionLogic(this IHockeyClientConfigurable @this, Func<UnobservedTaskExceptionEventArgs, bool> customFunc)
        {
            customUnobservedTaskExceptionFunc = customFunc;
            return @this;
        }

        static async void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            await HockeyClient.Current.AsInternal().HandleExceptionAsync(e.Exception);
            if (customUnobservedTaskExceptionFunc == null || customUnobservedTaskExceptionFunc(e))
            {
                Application.Current.Exit();
            }
        }

        #endregion

        #region Feedback

        /// <summary>
        /// Invoke this method to open the feedback UI where a user can send you a message including image attachments over the HockeyApp feedback system.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="initialUserName">[Optional] Username to prefill the name field</param>
        /// <param name="initialEMail">[Optional] Email to prefill the email field</param>
        public static void ShowFeedback(this IHockeyClient @this, string initialUserName = null, string initialEMail = null)
        {
            @this.AsInternal().CheckForInitialization();
            var flyout = new FeedbackFlyout();
            FeedbackManager.Current.InitialEmail = initialEMail;
            FeedbackManager.Current.InitialUsername = initialUserName;
            flyout.ShowIndependent();
            
        }

        /// <summary>
        /// Call in your app if user logs out. Deletes all persistently stored data like FeedbackThreadToken, and cached Message. .
        /// </summary>
        public static void LogoutFromFeedback(this IHockeyClient @this)
        {
            var settingValues = ApplicationData.Current.LocalSettings.Values;
            settingValues.RemoveValue(ConstantsUniversal.FeedbackThreadKey);
            FeedbackManager.Current.CurrentFeedbackFlyoutVM = new FeedbackFlyoutVM();
        }

        #endregion

    }
}
