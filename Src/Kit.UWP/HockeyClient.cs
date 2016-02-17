namespace Microsoft.HockeyApp
{
    using System;

    /// <summary>
    /// Send information to the HockeyApp service.
    /// </summary>
    public class HockeyClient : IHockeyClient
    {
        private static readonly Lazy<HockeyClient> lazy = new Lazy<HockeyClient>(() => new HockeyClient());

        /// <summary>
        /// Gets the current singleton instance of HockeyClient.
        /// </summary>
        public static IHockeyClient Current
        {
            get { return lazy.Value; }
        }

        /// <summary>
        /// Bootstraps HockeyApp SDK.
        /// </summary>
        /// <param name="appId">The application identifier, which is a unique hash string which is automatically created when you add a new application to HockeyApp.</param>
        public void Configure(string appId)
        {
            WindowsAppInitializer.InitializeAsync(appId, null);
        }

        /// <summary>
        /// Bootstraps HockeyApp SDK.
        /// </summary>
        /// <param name="appId">The application identifier, which is a unique hash string which is automatically created when you add a new application to HockeyApp.</param>
        /// <param name="configuration">Telemetry Configuration.</param>
        public void Configure(string appId, TelemetryConfiguration configuration)
        {
            WindowsAppInitializer.InitializeAsync(appId, configuration);
            Watson.WatsonIntegration.Integrate(new Guid(appId).ToString("D"));
        }
    }
}
