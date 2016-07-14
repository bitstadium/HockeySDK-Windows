using Microsoft.HockeyApp.DataContracts;
using System;
using System.Collections.Generic;

namespace Microsoft.HockeyApp
{
    /// <summary>
    /// Public Interface for HockeyClient. Used by static extension methods in platfomr-specific SDKs
    /// </summary>
    public interface IHockeyClient
    {
        /// <summary>
        /// Send a custom event for display in Events tab.
        /// </summary>
        /// <param name="eventName">Event name</param>
        void TrackEvent(string eventName);

        /// <summary>
        /// Send an event telemetry for display in Diagnostic Search and aggregation in Metrics Explorer.
        /// </summary>
        /// <param name="eventName">A name for the event.</param>
        /// <param name="properties">Named string values you can use to search and classify events.</param>
        /// <param name="metrics">Measurements associated with this event.</param>
        void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null);

        /// <summary>
        /// Send an <see cref="EventTelemetry"/> for display in Diagnostic Search and aggregation in Metrics Explorer.
        /// </summary>
        /// <param name="telemetry">An event log item.</param>
        void TrackEvent(EventTelemetry telemetry);

        /// <summary>
        /// Send a trace message for display in Diagnostic Search.
        /// </summary>
        /// <param name="message">Message to display.</param>
        void TrackTrace(string message);

        /// <summary>
        /// Send a trace message for display in Diagnostic Search.
        /// </summary>
        /// <param name="message">Message to display.</param>
        /// <param name="severityLevel">Trace severity level.</param>
        void TrackTrace(string message, SeverityLevel severityLevel);

        /// <summary>Mic
        /// Send a trace message for display in Diagnostic Search.
        /// </summary>
        /// <param name="message">Message to display.</param>
        /// <param name="properties">Named string values you can use to search and classify events.</param>
        void TrackTrace(string message, IDictionary<string, string> properties);

        /// <summary>
        /// Send a trace message for display in Diagnostic Search.
        /// </summary>
        /// <param name="message">Message to display.</param>
        /// <param name="severityLevel">Trace severity level.</param>
        /// <param name="properties">Named string values you can use to search and classify events.</param>
        void TrackTrace(string message, SeverityLevel severityLevel, IDictionary<string, string> properties);

        /// <summary>
        /// Send a trace message for display in Diagnostic Search.
        /// </summary>
        /// <param name="telemetry">Message with optional properties.</param>
        void TrackTrace(TraceTelemetry telemetry);

        /// <summary>
        /// Send metric telemetry for aggregation in Metric Explorer.
        /// </summary>
        /// <param name="name">Metric name.</param>
        /// <param name="value">Metric value.</param>
        /// <param name="properties">Named string values you can use to classify and filter metrics.</param>
        void TrackMetric(string name, double value, IDictionary<string, string> properties = null);

        /// <summary>
        /// Send a <see cref="MetricTelemetry"/> for aggregation in Metric Explorer.
        /// </summary>
        void TrackMetric(MetricTelemetry telemetry);

        /// <summary>
        /// Send information about the page viewed in the application.
        /// </summary>
        /// <param name="name">Name of the page.</param>
        void TrackPageView(string name);

        /// <summary>
        /// Send information about the page viewed in the application.
        /// </summary>
        void TrackPageView(PageViewTelemetry telemetry);

        /// <summary>
        /// Send a exception for display in Diagnostic Search.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        /// <param name="properties">Named string values you can use to classify and search for this exception.</param>
        void TrackException(Exception ex, IDictionary<string, string> properties = null);

        /// <summary>
        /// Send information about external dependency call in the application.
        /// </summary>
        /// <param name="dependencyName">External dependency name.</param>
        /// <param name="commandName">Dependency call command name.</param>
        /// <param name="startTime">The time when the dependency was called.</param>
        /// <param name="duration">The time taken by the external dependency to handle the call.</param>
        /// <param name="success">True if the dependency call was handled successfully.</param>
        void TrackDependency(string dependencyName, string commandName, DateTimeOffset startTime, TimeSpan duration, bool success);

        /// <summary>
        /// Send information about external dependency call in the application.
        /// </summary>
        void TrackDependency(DependencyTelemetry telemetry);

        /// <summary>
        /// Clears all buffers for this telemetry stream and causes any buffered data to be written to the underlying channel.
        /// </summary>
        void Flush();
    }
}
