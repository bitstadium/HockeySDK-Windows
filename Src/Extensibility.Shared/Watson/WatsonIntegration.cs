namespace Microsoft.HockeyApp.Watson
{
    using System;
    using System.Diagnostics.Tracing;

    internal class WatsonIntegration
    {
        internal static void Integrate(string hockeyAppIKey)
        {
            using (var eventSource = new PartnerTelemetryEventSource("Microsoft.HockeyApp.HockeyAppEventSource"))
            {
                eventSource.Write("HockeyAppWatsonIdentification", new HockeyAppIdentity() { PartA_iKey = hockeyAppIKey, HockeyAppCorrelationGuid = Guid.NewGuid().ToString() });
            }
        }

        [EventData]
        internal class HockeyAppIdentity
        {
            internal string PartA_iKey { get; set; }

            /// <summary>
            /// Gets or sets a GUID in HockeyApp UTC event as well.
            /// This GUID will permit Watson team to join, in the future, if they end up posting directly to Watson from inside the UWP app.
            /// </summary>
            internal string HockeyAppCorrelationGuid { get; set; }
        }
    }
}
