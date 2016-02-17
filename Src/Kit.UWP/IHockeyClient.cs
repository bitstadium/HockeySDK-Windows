namespace Microsoft.HockeyApp
{
    using System;
    using Microsoft.HockeyApp.Extensibility;

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
    }
}
