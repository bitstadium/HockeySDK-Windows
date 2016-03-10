namespace Microsoft.HockeyApp
{
    using System;

    /// <summary>
    /// Send information to the HockeyApp service.
    /// </summary>
    public class HockeyClient : IHockeyClient
    {
        private static readonly Lazy<HockeyClient> lazy = new Lazy<HockeyClient>(() => new HockeyClient());

        private TelemetryClient telemetryClient;

        /// <summary>
        /// Gets the current singleton instance of HockeyClient.
        /// </summary>
        public static HockeyClient Current
        {
            get { return lazy.Value; }
        }

        internal TelemetryClient TelemetryClient
        {
            set
            {
                this.telemetryClient = value;
            }
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
        }

        /// <summary>
        /// Send a custom event for display in Events tab.
        /// </summary>
        /// <param name="eventName">Event name</param>
        public void TrackEvent(string eventName)
        {
            if (this.telemetryClient != null)
            {
                this.telemetryClient.TrackEvent(eventName);
            }
        }
    }
}
