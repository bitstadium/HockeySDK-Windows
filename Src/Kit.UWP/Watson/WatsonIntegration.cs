namespace Microsoft.HockeyApp.Watson
{
    using System.Diagnostics.Tracing;

    internal class WatsonIntegration
    {
        internal static void Integrate(string hockeyAppIKey)
        {
            using (var eventSource = new Microsoft.Diagnostics.Telemetry2.PartnerTelemetryEventSource("Microsoft.HockeyApp.HockeyAppEventSource"))
            {
                eventSource.Write("HockeyAppWatsonIdentification", new HockeyAppIdentity() { PartA_iKey = hockeyAppIKey });
            }
        }

        [EventData]
        internal class HockeyAppIdentity
        {
            internal string PartA_iKey { get; set; }
        }
    }
}
