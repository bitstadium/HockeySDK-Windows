using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.HockeyApp.Services;
using Microsoft.HockeyApp.Services.Device;

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
        /// <returns>Instance object.</returns>
        public static IHockeyClientConfigurable Configure(this IHockeyClient @this, string identifier)
        {
            return @this.Configure(identifier, null, null, null, DictionarySettings.Current.LocalSettings, DictionarySettings.Current.RoamingSettings);
        }

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
        public static IHockeyClientConfigurable Configure(this IHockeyClient @this, string identifier, IDictionary<string, object> localApplicationSettings, IDictionary<string, object> roamingApplicationSettings, bool keepRunningAfterException = false)
        {
            return @this.Configure(identifier, null, null, null, localApplicationSettings, roamingApplicationSettings, keepRunningAfterException);
        }

        /// <summary>
        /// Configures HockeyClient.
        /// </summary>
        /// <param name="this">HockeyClient object.</param>
        /// <param name="identifier">Identfier.</param>
        /// <param name="appId">Namespace of main app type.</param>
        /// <param name="appVersion">Four field app version.</param>
        /// <param name="storeRegion">storeRegion.</param>
        /// <param name="localApplicationSettings">A persistable collection of settings equivalent to:
        /// https://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k(Windows.Storage.ApplicationData);k(TargetFrameworkMoniker-.NETCore,Version%3Dv5.0);k(DevLang-csharp)&rd=true</param>
        /// <param name="roamingApplicationSettings">A persistable collection of settings equivalent to:
        /// https://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k(Windows.Storage.ApplicationData);k(TargetFrameworkMoniker-.NETCore,Version%3Dv5.0);k(DevLang-csharp)&rd=true.</param>
        /// <param name="keepRunningAfterException">Keep running after exception.</param>
        /// <returns>Instance object.</returns>
        public static IHockeyClientConfigurable Configure(this IHockeyClient @this, string identifier, string appId, string appVersion, string storeRegion, IDictionary<string, object> localApplicationSettings, IDictionary<string, object> roamingApplicationSettings, bool keepRunningAfterException = false)
        {
            if (localApplicationSettings == null) throw new ArgumentNullException("localApplicationSettings");
            if (roamingApplicationSettings == null) throw new ArgumentNullException("roamingApplicationSettings");

            var deviceService = new DeviceService();
            @this.AsInternal().PlatformHelper = new HockeyPlatformHelperWinForms(deviceService);
            @this.AsInternal().AppIdentifier = identifier;

            ServiceLocator.AddService<IPlatformService>(new PlatformService(localApplicationSettings, roamingApplicationSettings));
            ServiceLocator.AddService<IApplicationService>(new ApplicationService(appId, appVersion, storeRegion));
            ServiceLocator.AddService<IHttpService>(new WinFormsHttpService());
            ServiceLocator.AddService<IDeviceService>(deviceService);
            ServiceLocator.AddService<BaseStorageService>(new StorageService());
            ServiceLocator.AddService<IUnhandledExceptionTelemetryModule>(new UnhandledExceptionTelemetryModule(keepRunningAfterException));

            var config = new TelemetryConfiguration()
            {
#if DEBUG
                EnableDiagnostics = true,
#endif
                InstrumentationKey = identifier
            };

            WindowsAppInitializer.InitializeAsync(identifier, config).ContinueWith(t =>
            {
                object userId = null;
                if (roamingApplicationSettings.TryGetValue("HockeyAppUserId", out userId) && userId != null)
                {
                    ((IHockeyClientConfigurable)@this).SetContactInfo(userId.ToString(), null);
                }
            });

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

        static async void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            await HockeyClient.Current.AsInternal().HandleExceptionAsync(e.Exception);
        }

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
    }
}
