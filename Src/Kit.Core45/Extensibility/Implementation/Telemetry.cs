namespace Microsoft.HockeyApp.Extensibility.Implementation
{
    using System.Globalization;
    using Channel;
    using DataContracts;

    // TODO: Move Telemetry class to DataContracts namespace for discoverability.
    internal static class Telemetry
    {
        internal const string DevModeTelemetryNamePrefix = "Microsoft.ApplicationInsights.Dev.";

        public static void WriteEnvelopeProperties(this ITelemetry telemetry, IJsonWriter json)
        {
            json.WriteProperty("time", telemetry.Timestamp.UtcDateTime.ToString("o", CultureInfo.InvariantCulture));
            json.WriteProperty("seq", telemetry.Sequence);
            ((IJsonSerializable)telemetry.Context).Serialize(json);
        }

        public static void WriteTelemetryName(this ITelemetry telemetry, IJsonWriter json, string telemetryName)
        {
            // A different event name prefix is sent for normal mode and developer mode.
            bool isDevMode = false;
            string devModeProperty;
            var telemetryWithProperties = telemetry as ISupportProperties;
            if (telemetryWithProperties != null && telemetryWithProperties.Properties.TryGetValue("DeveloperMode", out devModeProperty))
            {
                bool.TryParse(devModeProperty, out isDevMode);
            }

            // Format the event name using the following format:
            // Microsoft.[ProductName].[Dev].<normalized-instrumentation-key>.<event-type>
            var eventName = string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "{0}{1}{2}",
                isDevMode ? DevModeTelemetryNamePrefix : TelemetryItemExtensions.TelemetryNamePrefix,
                NormalizeInstrumentationKey(telemetry.Context.InstrumentationKey),
                telemetryName);
            json.WriteProperty("name", eventName);
        }

        /// <summary>
        /// Normalize instrumentation key by removing dashes ('-') and making string in the lowercase.
        /// In case no InstrumentationKey is available just return empty string.
        /// In case when InstrumentationKey is available return normalized key + dot ('.')
        /// as a separator between instrumentation key part and telemetry name part.
        /// </summary>
        private static string NormalizeInstrumentationKey(string instrumentationKey)
        {
            if (instrumentationKey.IsNullOrWhiteSpace())
            {
                return string.Empty;
            }

            return instrumentationKey.Replace("-", string.Empty).ToLowerInvariant() + ".";
        }
    }
}
