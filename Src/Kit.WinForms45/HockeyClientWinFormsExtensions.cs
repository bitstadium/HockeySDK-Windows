using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;

using Microsoft.HockeyApp.Services;

namespace Microsoft.HockeyApp
{
    /// <summary>
    /// HockeyClient Extension for WinForms.
    /// </summary>
    public static class HockeyClientWinFormsExtensions
    {
        /// <summary>
        /// Configures HockeyClient.
        /// </summary>
        /// <param name="this">HockeyClient object.</param>
        /// <param name="identifier">Identfier.</param>
        /// <param name="localApplicationSettings">A persistable collection of settings equivalent to:
        /// https://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k(Windows.Storage.ApplicationData);k(TargetFrameworkMoniker-.NETCore,Version%3Dv5.0);k(DevLang-csharp)&rd=true</param>
        /// <param name="roamingApplicationSettings">A persistable collection of settings equivalent to:
        /// https://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k(Windows.Storage.ApplicationData);k(TargetFrameworkMoniker-.NETCore,Version%3Dv5.0);k(DevLang-csharp)&rd=true.</param>
        /// <param name="keepRunningAfterException">Keep running after exception.</param>
        /// <returns>Instance object.</returns>
        public static IHockeyClientConfigurable Configure(this IHockeyClient @this, string identifier, IDictionary<string, object> localApplicationSettings, IDictionary<string, object> roamingApplicationSettings, bool keepRunningAfterException)
        {
            @this.AsInternal().PlatformHelper = new HockeyPlatformHelperWinForms();
            @this.AsInternal().AppIdentifier = identifier;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            if (keepRunningAfterException)
            {
                Application.ThreadException += Current_ThreadException;
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            }

            ServiceLocator.AddService<IPlatformService>(new PlatformService(localApplicationSettings, roamingApplicationSettings));
            ServiceLocator.AddService<IHttpService>(new WinFormsHttpService());
            ServiceLocator.AddService<IDeviceService>(new DeviceService());
            ServiceLocator.AddService<BaseStorageService>(new StorageService());
            //ServiceLocator.AddService<IUnhandledExceptionTelemetryModule>(new UnhandledExceptionTelemetryModule());

            var config = new TelemetryConfiguration()
            {
#if DEBUG
                EnableDiagnostics = true,
#endif
                InstrumentationKey = identifier
            };
            WindowsAppInitializer.InitializeAsync(identifier, config);

            return (IHockeyClientConfigurable)@this;
        }

        /// <summary>
        /// Use this if your WinForms app is a UWP bridge (aka Centennial) app
        /// </summary>
        /// <param name="this"></param>
        /// <param name="appId"></param>
        /// <param name="appVersion"></param>
        /// <param name="storeRegion"></param>
        /// <returns></returns>
        public static IHockeyClientConfigurable SetApplicationDetails(this IHockeyClientConfigurable @this, string appId, string appVersion, string storeRegion)
        {
            ServiceLocator.AddService<IApplicationService>(new ApplicationService(appId, appVersion, storeRegion));

            return (IHockeyClientConfigurable)@this;
        }

        /// <summary>
        /// Adds the handler for UnobservedTaskExceptions
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static IHockeyClientConfigurable RegisterDefaultUnobservedTaskExceptionHandler(this IHockeyClientConfigurable @this)
        {
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            return @this;
        }

        /// <summary>
        /// Removes the handler for UnobservedTaskExceptions
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static IHockeyClientConfigurable UnregisterDefaultUnobservedTaskExceptionHandler(this IHockeyClientConfigurable @this)
        {
            TaskScheduler.UnobservedTaskException -= TaskScheduler_UnobservedTaskException;
            return @this;
        }

        static async void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            if (ex != null)
            {
                await HockeyClient.Current.AsInternal().HandleExceptionAsync(ex);
            }
        }

        static async void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            await HockeyClient.Current.AsInternal().HandleExceptionAsync(e.Exception);
        }

        static async void Current_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            await HockeyClient.Current.AsInternal().HandleExceptionAsync(e.Exception);
        }

        #region CrashHandling

        /// <summary>
        /// Send crashes to the HockeyApp server
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static async Task<bool> SendCrashesAsync(this IHockeyClient @this)
        {
            @this.AsInternal().CheckForInitialization();
            bool result = await @this.AsInternal().SendCrashesAndDeleteAfterwardsAsync().ConfigureAwait(false);
            return result;
        }

        #endregion

    }
}
