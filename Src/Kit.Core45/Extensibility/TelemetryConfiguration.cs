namespace Microsoft.HockeyApp
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Channel;
    using DataContracts;
    using Extensibility;
    using Extensibility.Implementation;
    using Extensibility.Implementation.Tracing;

    /// <summary>
    /// Encapsulates the global telemetry configuration typically loaded from the configuration file.
    /// </summary>
    /// <remarks>
    /// All <see cref="TelemetryContext"/> objects are initialized using the <see cref="Active"/> 
    /// telemetry configuration provided by this class.
    /// </remarks>
    public sealed class TelemetryConfiguration : IDisposable
    {
        private static TelemetryConfiguration active;
        private readonly SnapshottingList<IContextInitializer> contextInitializers = new SnapshottingList<IContextInitializer>();
        private readonly SnapshottingList<ITelemetryInitializer> telemetryInitializers = new SnapshottingList<ITelemetryInitializer>();

        private string instrumentationKey = string.Empty;
        private bool disableTelemetry = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryConfiguration" /> class.
        /// </summary>
        public TelemetryConfiguration()
        {
            this.Collectors = WindowsCollectors.Metadata | WindowsCollectors.Session | WindowsCollectors.UnhandledException;
        }

        /// <summary>
        /// Gets or sets Windows Collectors.
        /// </summary>
        public WindowsCollectors Collectors
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets Endpoint address.
        /// </summary>
        public string EndpointAddress
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the internal diagnostics is enabled.
        /// </summary>
        public bool EnableDiagnostics
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the active <see cref="TelemetryConfiguration"/> instance loaded from the configuration file. 
        /// If the configuration file does not exist, the active configuration instance is initialized with minimum defaults 
        /// needed to send telemetry to Application Insights.
        /// </summary>
        internal static TelemetryConfiguration Active
        {
            get
            {
                if (active == null)
                {
                    if (active == null)
                    {
                        active = new TelemetryConfiguration();
                        TelemetryConfigurationFactory.Instance.Initialize(active);
                    }
                }

                return active;
            }

            set
            {
                active = value;
            }
        }

        /// <summary>
        /// Gets or sets the default instrumentation key for the application.
        /// </summary>
        /// <exception cref="ArgumentNullException">The new value is null.</exception>
        internal string InstrumentationKey
        {
            get
            {
                return this.instrumentationKey;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                this.instrumentationKey = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether sending of telemetry to Application Insights is disabled.
        /// </summary>
        /// <remarks>
        /// This disable tracking setting value is used by default by all instances created in the application. 
        /// </remarks>
        internal bool DisableTelemetry
        {
            get
            {
                return this.disableTelemetry;
            }

            set
            {
                // Log the state of tracking 
                if (value)
                {
                    CoreEventSource.Log.LogVerbose("Telemetry tracking was disabled.Message is dropped.");
                }
                else
                {
                    CoreEventSource.Log.LogVerbose("Telemetry tracking was enabled. Messages are being logged.");
                }

                this.disableTelemetry = value;
            }
        }

        /// <summary>
        /// Gets the list of <see cref="IContextInitializer"/> objects that supply additional information about application.
        /// </summary>
        /// <remarks>
        /// Context initializers extend Application Insights telemetry collection by supplying additional information 
        /// about application environment, such as <see cref="TelemetryContext.User"/> or <see cref="TelemetryContext.Device"/> 
        /// information that remains constant during application lifetime. A client invokes context 
        /// initializers to obtain initial property values for <see cref="TelemetryContext"/> object during its construction.
        /// The default list of context initializers is provided by the Application Insights NuGet packages and loaded from 
        /// the configuration file located in the application directory. 
        /// </remarks>
        internal IList<IContextInitializer> ContextInitializers
        {
            get { return this.contextInitializers; }
        }

        /// <summary>
        /// Gets the list of <see cref="ITelemetryInitializer"/> objects that supply additional information about telemetry.
        /// </summary>
        /// <remarks>
        /// Telemetry initializers extend Application Insights telemetry collection by supplying additional information 
        /// about individual <see cref="ITelemetry"/> items, such as <see cref="ITelemetry.Timestamp"/>. A telemetry client
        /// invokes telemetry initializers each time track method is called.
        /// The default list of telemetry initializers is provided by the Application Insights NuGet packages and loaded from 
        /// the configuration file located in the application directory. 
        /// </remarks>
        internal IList<ITelemetryInitializer> TelemetryInitializers
        {
            get { return this.telemetryInitializers; }
        }
        
        /// <summary>
        /// Gets or sets the telemetry channel.
        /// </summary>
        internal ITelemetryChannel TelemetryChannel { get; set; }

        /// <summary>
        /// Releases resources used by the current instance of the <see cref="TelemetryConfiguration"/> class.
        /// </summary>
        public void Dispose()
        {
            // TODO: Implement a Finalizer to dispose this class.
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Creates a new <see cref="TelemetryConfiguration"/> instance loaded from the configuration file.
        /// If the configuration file does not exist, the new configuration instance is initialized with minimum defaults 
        /// needed to send telemetry to Application Insights.
        /// </summary>
        internal static TelemetryConfiguration CreateDefault()
        {
            var configuration = new TelemetryConfiguration();
            TelemetryConfigurationFactory.Instance.Initialize(configuration);

            return configuration;
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                Interlocked.CompareExchange(ref active, null, this);

                ITelemetryChannel telemetryChannel = this.TelemetryChannel;
                if (telemetryChannel != null)
                {
                    telemetryChannel.Dispose();
                }
            }
        }
    }
}