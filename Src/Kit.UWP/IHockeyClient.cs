namespace Microsoft.HockeyApp
{
    /// <summary>
    /// Public Interface for HockeyClient. 
    /// </summary>
    public interface IHockeyClient
    {
        /// <summary>
        /// Bootstraps HockeyApp SDK.
        /// </summary>
        /// <param name="appId">App ID.</param>
        /// <param name="configuration">Telemetry configuration.</param>
        void Configure(string appId, TelemetryConfiguration configuration = null);

        /// <summary>
        /// Send a custom event for display in Events tab.
        /// </summary>
        /// <param name="eventName">Event name</param>
        void TrackEvent(string eventName);
    }
}
