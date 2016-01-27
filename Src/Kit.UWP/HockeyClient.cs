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
        /// <param name="appId">App ID.</param>
        /// <param name="endpointAddress">The http address where the telemetry is sent.</param>
        public void Configure(string appId, string endpointAddress = null)
        {
            WindowsAppInitializer.InitializeAsync(appId, WindowsCollectors.Metadata | WindowsCollectors.Session | WindowsCollectors.UnhandledException, endpointAddress);
            Watson.WatsonIntegration.Integrate(appId);
        }
    }
}
