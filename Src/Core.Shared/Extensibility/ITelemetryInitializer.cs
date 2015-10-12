namespace Microsoft.ApplicationInsights.Extensibility
{
    using Channel;
    using DataContracts;

    /// <summary>
    /// Represents an object that initializes <see cref="ITelemetry"/> objects.
    /// </summary>
    /// <remarks>
    /// The <see cref="TelemetryContext"/> instances use <see cref="ITelemetryInitializer"/> objects to 
    /// automatically initialize properties of the <see cref="ITelemetry"/> objects.
    /// </remarks>
    public interface ITelemetryInitializer
    {
        /// <summary>
        /// Initializes properties of the specified <see cref="ITelemetry"/> object.
        /// </summary>
        void Initialize(ITelemetry telemetry);
    }
}
