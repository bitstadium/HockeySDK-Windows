namespace Microsoft.HockeyApp.Watson
{
    internal class WatsonIntegration
    {
        internal static void Integrate(string hockeyAppIKey)
        {
            using (var eventSource = new PartnerTelemetryEventSource("Microsoft.HockeyApp.HockeyAppEventSource"))
            {
                eventSource.WriteIntegratationEvent(hockeyAppIKey);
            }
        }
    }
}
