namespace Microsoft.ApplicationInsights.Channel
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility.Implementation.Tracing;
    using System.Diagnostics.Tracing;

    /// <summary>
    /// Encapsulates logic for sending a telemetry as a Common Schema 2.0 event.
    /// </summary>
    public sealed class EventSourceWriter : IDisposable
    {
        private readonly string instrumentationKey;
        private readonly EventSource eventSource;
        private readonly EventSourceOptions eventSourceOptions;
        private bool disposed;

        internal EventSourceWriter(string instrumentationKey, bool developerMode = false)
        {
            this.instrumentationKey = instrumentationKey;

            string normalizedInstrumentationKey = RemoveInvalidInstrumentationKeyChars(this.instrumentationKey.ToLowerInvariant());

            string telemetryNamePrefix; 
            string telemetryGroup;

            if (developerMode)
            {
                telemetryNamePrefix = Constants.DevModeTelemetryNamePrefix;
                telemetryGroup = Constants.DevModeTelemetryGroup;
            }
            else
            {
                telemetryNamePrefix = Constants.TelemetryNamePrefix;
                telemetryGroup = Constants.TelemetryGroup;
            }

            this.eventSource = new EventSource(
                telemetryNamePrefix + normalizedInstrumentationKey,
                EventSourceSettings.EtwSelfDescribingEventFormat,
                Constants.EventSourceGroupTraitKey,
                telemetryGroup);

            this.eventSourceOptions = new EventSourceOptions() { Keywords = (EventKeywords)0x2000000000000 };
        }

        /// <summary>
        /// Gets the underlying EventSource (ETW) ID. Exposed for Unit Tests purposes.
        /// </summary>
        internal Guid ProviderId
        {
            get
            {
                return this.eventSource.Guid;
            }
        }

        /// <summary>
        /// Gets the underlying EventSource (ETW) Name. Exposed for Unit Tests purposes.
        /// </summary>
        internal string ProviderName
        {
            get
            {
                return this.eventSource.Name;
            }
        }

        /// <summary>
        /// Gets the instrumentation key for this writer. Exposed for Unit Tests purposes.
        /// </summary>
        internal string InstrumentationKey
        {
            get
            {
                return this.instrumentationKey;
            }
        }

        /// <summary>
        /// Releases resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal void WriteTelemetry(ITelemetry telemetryItem)
        {
            if (telemetryItem == null)
            {
                CoreEventSource.Log.LogVerbose("telemetryItem param is null in EventSourceWriter.WriteTelemetry()");                
                return;
            }

            if (this.eventSource.IsEnabled())
            {
                if (telemetryItem is EventTelemetry)
                {
                    EventTelemetry eventTelemetry = telemetryItem as EventTelemetry;
                    this.WriteEvent(EventTelemetry.TelemetryName, eventTelemetry.Context, eventTelemetry.Data);
                }
                else if (telemetryItem is ExceptionTelemetry)
                {
                    ExceptionTelemetry exceptiontelemetry = telemetryItem as ExceptionTelemetry;
                    this.WriteEvent(ExceptionTelemetry.TelemetryName, exceptiontelemetry.Context, exceptiontelemetry.Data);
                }
                else if (telemetryItem is MetricTelemetry)
                {
                    MetricTelemetry metricTelemetry = telemetryItem as MetricTelemetry;
                    this.WriteEvent(MetricTelemetry.TelemetryName, metricTelemetry.Context, metricTelemetry.Data);
                }
                else if (telemetryItem is PageViewTelemetry)
                {
                    PageViewTelemetry pageViewTelemetry = telemetryItem as PageViewTelemetry;
                    this.WriteEvent(PageViewTelemetry.TelemetryName, pageViewTelemetry.Context, pageViewTelemetry.Data);
                }
                else if (telemetryItem is DependencyTelemetry)
                {
                    DependencyTelemetry remoteDependencyTelemetry = telemetryItem as DependencyTelemetry;
                    this.WriteEvent(DependencyTelemetry.TelemetryName, remoteDependencyTelemetry.Context, remoteDependencyTelemetry.Data);
                }
                else if (telemetryItem is RequestTelemetry)
                {
                    RequestTelemetry requestTelemetry = telemetryItem as RequestTelemetry;
                    this.WriteEvent(RequestTelemetry.TelemetryName, requestTelemetry.Context, requestTelemetry.Data);
                }
                else if (telemetryItem is SessionStateTelemetry)
                {
                    SessionStateTelemetry sessionStateTelemetry = telemetryItem as SessionStateTelemetry;
                    sessionStateTelemetry.Data.state = (Microsoft.ApplicationInsights.Extensibility.Implementation.External.SessionState)sessionStateTelemetry.State;
                    this.WriteEvent(SessionStateTelemetry.TelemetryName, sessionStateTelemetry.Context, sessionStateTelemetry.Data);
                }
                else if (telemetryItem is TraceTelemetry)
                {
                    TraceTelemetry traceTelemetry = telemetryItem as TraceTelemetry;
                    this.WriteEvent(TraceTelemetry.TelemetryName, traceTelemetry.Context, traceTelemetry.Data);
                }
                else
                {
                    string msg = string.Format(CultureInfo.InvariantCulture, "Unknown telemtry type: {0}", telemetryItem.GetType());                    
                    CoreEventSource.Log.LogVerbose(msg);
                }
            }
        }

        internal void WriteEvent<T>(string eventName, TelemetryContext context, T data)
        {
            this.eventSource.Write(eventName, this.eventSourceOptions, new { PartA_iKey = this.instrumentationKey, PartA_Tags = context.Tags, _B = data });
        }

        private static string RemoveInvalidInstrumentationKeyChars(string input)
        {
            Regex r = new Regex("(?:[^a-z0-9.])", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            return r.Replace(input, string.Empty);
        }

        private void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                this.eventSource.Dispose();
            }

            this.disposed = true;
        }
    }
}