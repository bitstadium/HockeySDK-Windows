namespace Microsoft.HockeyApp.Watson
{
    internal class WatsonIntegration
    {
        internal static void Integrate(string hockeyAppIKey)
        {
            using (var eventSource = new Microsoft.Diagnostics.Telemetry2.PartnerTelemetryEventSource("HockeyAppEventSource"))
            {
                eventSource.Write("HockeyAppWatsonIdentification", new { PartA_iKey = hockeyAppIKey });
            }
        }
    }
}
