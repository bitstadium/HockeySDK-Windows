namespace Microsoft.HockeyApp
{
    using Extensibility.Windows;
    using Services;
    using Services.Device;

    /// <summary>
    /// Send information to the HockeyApp service.
    /// </summary>
    public static class HockeyClientExtensionsUwp
    {
        /// <summary>
        /// Bootstraps HockeyApp SDK.
        /// </summary>
        /// <param name="this"><see cref="HockeyClient"/></param>
        /// <param name="appId">The application identifier, which is a unique hash string which is automatically created when you add a new application to HockeyApp.</param>
        public static IHockeyClientConfigurable Configure(this IHockeyClient @this, string appId)
        {
            return Configure(@this, appId, null) as IHockeyClientConfigurable;
        }

        /// <summary>
        /// Bootstraps HockeyApp SDK.
        /// </summary>
        /// <param name="this"><see cref="HockeyClient"/></param>
        /// <param name="appId">The application identifier, which is a unique hash string which is automatically created when you add a new application to HockeyApp service.</param>
        /// <param name="configuration">Telemetry Configuration.</param>
        public static IHockeyClientConfigurable Configure(this IHockeyClient @this, string appId, TelemetryConfiguration configuration)
        {
            if (@this.AsInternal().TestAndSetIsConfigured())
            {
                return @this as IHockeyClientConfigurable;
            }
            ServiceLocator.AddService<BaseStorageService>(new StorageService());
            ServiceLocator.AddService<IApplicationService>(new ApplicationService());
            ServiceLocator.AddService<IDeviceService>(new DeviceService());
            ServiceLocator.AddService<Services.IPlatformService>(new PlatformService());
            ServiceLocator.AddService<IHttpService>(new HttpClientTransmission());
            ServiceLocator.AddService<IUnhandledExceptionTelemetryModule>(new UnhandledExceptionTelemetryModule());
            WindowsAppInitializer.InitializeAsync(appId, configuration);
            return @this as IHockeyClientConfigurable;
        }
    }
}
