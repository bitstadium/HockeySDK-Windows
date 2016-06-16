namespace Microsoft.HockeyApp
{
    using Extensibility.Implementation;
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
        public static void Configure(this IHockeyClient @this, string appId)
        {
            Configure(@this, appId, null);
        }

        /// <summary>
        /// Bootstraps HockeyApp SDK.
        /// </summary>
        /// <param name="this"><see cref="HockeyClient"/></param>
        /// <param name="appId">The application identifier, which is a unique hash string which is automatically created when you add a new application to HockeyApp.</param>
        /// <param name="configuration">Telemetry Configuration.</param>
        public static void Configure(this IHockeyClient @this, string appId, TelemetryConfiguration configuration)
        {
            ServiceLocator.AddService<BaseStorageService>(new StorageService());
            ServiceLocator.AddService<IApplicationService>(new ApplicationService());
            ServiceLocator.AddService<IDeviceService>(new DeviceService());
            ServiceLocator.AddService<Services.IPlatformService>(new PlatformService());
            ServiceLocator.AddService<IHttpService>(new HttpClientTransmission());
            ServiceLocator.AddService<IUnhandledExceptionTelemetryModule>(new UnhandledExceptionTelemetryModule());
            WindowsAppInitializer.InitializeAsync(appId, configuration);
        }
    }
}
