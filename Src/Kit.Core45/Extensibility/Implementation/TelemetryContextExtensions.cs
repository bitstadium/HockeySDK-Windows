namespace Microsoft.HockeyApp.Extensibility.Implementation
{
    using DataContracts;

    /// <summary>
    /// Extension methods for TelemetryContext.
    /// </summary>
    internal static class TelemetryContextExtensions
    {
        /// <summary>
        /// Returns TelemetryContext's Internal context.
        /// </summary>
        /// <param name="context">Telemetry context to get Internal context for.</param>
        /// <returns>Internal context for TelemetryContext.</returns>
        internal static InternalContext GetInternalContext(this TelemetryContext context)
        {
            return context.Internal;
        }
    }
}
