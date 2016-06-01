namespace Microsoft.HockeyApp.TestFramework
{
    using Channel;
    using Extensibility;

    /// <summary>
    /// A stub of <see cref="ITelemetryInitializer"/>.
    /// </summary>
    internal sealed class StubTelemetryInitializer : ITelemetryInitializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StubTelemetryInitializer"/> class.
        /// </summary>
        public StubTelemetryInitializer()
        {
            this.OnInitialize = telemetry => { };
        }

        /// <summary>
        /// Gets or sets the callback invoked by the <see cref="Initialize"/> method.
        /// </summary>
        internal TelemetryAction OnInitialize { get; set; }

        /// <summary>
        /// Implements the <see cref="ITelemetryInitializer.Initialize"/> method by invoking the <see cref="OnInitialize"/> callback.
        /// </summary>
        public void Initialize(ITelemetry telemetry)
        {
            this.OnInitialize(telemetry);
        }
    }
}
