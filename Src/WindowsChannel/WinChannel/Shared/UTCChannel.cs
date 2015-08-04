#if !Wp80 && !NET35
namespace Microsoft.ApplicationInsights.Channel.UniversalTelemetryChannel
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.Diagnostics.Tracing;

    /// <summary>
    /// Represents a communication channel for sending telemetry to Application Insights via UTC (Windows Universal Telemetry Client).
    /// </summary>
    public sealed class OutOfProcessTelemetryChannel : ITelemetryChannel, IDisposable
    {
        private readonly ConcurrentDictionary<string/*instrumentationKey*/, EventSourceWriter> eventSourceWriters;

        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutOfProcessTelemetryChannel"/> class.
        /// </summary>
        public OutOfProcessTelemetryChannel()
        {
            this.eventSourceWriters = new ConcurrentDictionary<string, EventSourceWriter>();
        }

        /// <summary>
        /// Gets or sets a value indicating whether developer mode of telemetry transmission is enabled.
        /// When developer mode is True, <see cref="OutOfProcessTelemetryChannel"/> sends telemetry to Application Insights immediately 
        /// during the entire lifetime of the application. When developer mode is False, <see cref="OutOfProcessTelemetryChannel"/>
        /// respects production sending policies defined by other properties.
        /// </summary>
        public bool DeveloperMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the endpoint address. This property is ignored. 
        /// </summary>
        public string EndpointAddress { get; set; }

        /// <summary>
        /// Returns true if the channel is available to use.
        /// </summary>
        public static bool IsAvailable()
        {
            using (EventSource utcPresenceEventSource = new EventSource("Microsoft-Windows-UTC-Presence"))
            {
                return utcPresenceEventSource.IsEnabled();
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Sends an instance of ITelemetry through the channel.
        /// </summary>
        public void Send(ITelemetry item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            EventSourceWriter eventSourceWriter = this.GetEventSourceWriter(item.Context.InstrumentationKey);
            eventSourceWriter.WriteTelemetry(item);
        }

        /// <summary>
        /// No-op because every <see cref="Send"/> method is immediately calling UTC. So every call immediately "flushed" to the UTC agent. 
        /// </summary>
        public void Flush()
        {
        }

        internal EventSourceWriter GetEventSourceWriter(string instrumentationKey)
        {
            EventSourceWriter eventSourceWriter = this.eventSourceWriters.GetOrAdd(
                instrumentationKey,
                key => new EventSourceWriter(key, this.DeveloperMode));

            return eventSourceWriter;
        }
        
        private void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                foreach (var item in this.eventSourceWriters)
                {
                    item.Value.Dispose();
                }
            }

            this.disposed = true;
        }
    }
}
#endif