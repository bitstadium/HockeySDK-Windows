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
        /// <param name="endpointAddress">The http address where the telemetry is sent.</param>
        void Configure(string appId, string endpointAddress);
    }
}
