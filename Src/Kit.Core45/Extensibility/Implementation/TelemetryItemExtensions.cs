namespace Microsoft.HockeyApp.Extensibility.Implementation
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Channel;

    internal static class TelemetryItemExtensions
    {
        internal const string TelemetryNamePrefix = "Microsoft.ApplicationInsights.";

        internal static string GetTelemetryFullName(this ITelemetry item, string envelopeName)
        {
            return TelemetryNamePrefix + item.Context.InstrumentationKey + "|" + envelopeName;
        }
    }
}
