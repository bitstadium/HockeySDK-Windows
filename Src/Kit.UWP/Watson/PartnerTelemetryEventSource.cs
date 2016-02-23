#if PARTNERTELEMETRYEVENTSOURCE_USE_NUGET
using Microsoft.Diagnostics.Tracing;  
#else
using System.Diagnostics.Tracing;
#endif

/*  
NOTES:  
  
Partner must update the PartnerProviderGuid constant with the  
assigned provider group ID. In addition, your project must do one of  
the following:  
  
    1. Target .NET 4.6 or later.  
    2. Add a reference to the EventSource NuGet version 1.1.23 or later,  
       and enable the conditional compilation symbol  
       PARTNERTELEMETRYEVENTSOURCE_USE_NUGET.  
  
To add an iKey to your event, add an event field named "PartA_iKey" with a  
string value. For example:  
  
    eventSource.Write(  
        "MyEventName",  
        new {  
            PartA_iKey = "MyIKeyValue",  
            Field1 = x,  
            Field2 = y,  
            ... etc.  
        });  
*/

namespace Microsoft.HockeyApp.Watson
{
    /// <summary>  
    /// <para>  
    /// An Asimov-enabled EventSource. This inherits from EventSource, and is  
    /// exactly the same as EventSource except that it forces Asimov-compatible  
    /// construction (it always enables EtwSelfDescribingEventFormat and joins the  
    /// MicrosoftPartnerTelemetry group). It also provides several Asimov-specific  
    /// constants.  
    /// </para>  
    /// <para>  
    /// Note that this class DOES NOT automatically add any keywords to your events.  
    /// Even when using this class, events will be ignored by UTC unless they include  
    /// one of the telemetry keywords. Each event that you want to send to UTC must  
    /// have one (and only one) of the following keywords set in  
    /// eventSourceOptions.Keywords: TelemetryKeyword, MeasuresKeyword, or  
    /// CriticalDataKeyword.  
    /// </para>  
    /// <para>  
    /// When including this class in your project, you may define the following  
    /// conditional-compilation symbols to adjust the default behaviors:  
    /// </para>  
    /// <para>  
    /// PARTNERTELEMETRYEVENTSOURCE_USE_NUGET - use Microsoft.Diagnostics.Tracing instead  
    /// of System.Diagnostics.Tracing.  
    /// </para>  
    /// <para>  
    /// PARTNERTELEMETRYEVENTSOURCE_PUBLIC - define PartnerTelemetryEventSource as public instead  
    /// of internal.  
    /// </para>  
    /// </summary>  
#if PARTNERTELEMETRYEVENTSOURCE_PUBLIC
    public  
#else
    internal
#endif
        class PartnerTelemetryEventSource : EventSource
    {
        /// <summary>  
        /// Partner must update this constant to the assigned provider group ID.  
        /// ToDo: Guid must be generated using EtGuid.exe Microsoft.HockeyApp.HockeyAppEventSource, see http://blogs.msdn.com/b/dcook/archive/2015/09/08/etw-provider-names-and-guids.aspx.
        /// Currently, for prototype using Windows Internal Group Guid provided by sorino@microsoft.com. 
        /// </summary>  
        private const string PartnerProviderGuid = "4f50731a-89cf-4782-b3e0-dce8c90476ba";

        /// <summary>  
        /// Keyword 0x0000100000000000 is reserved for future definition by UTC. Do  
        /// not use keyword 0x0000100000000000 for telemetry-enabled ETW events.  
        /// </summary>  
        public const EventKeywords Reserved44Keyword = (EventKeywords)0x0000100000000000;

        /// <summary>  
        /// Add TelemetryKeyword to eventSourceOptions.Keywords to indicate that  
        /// an event is for general-purpose telemetry.  
        /// This keyword should not be combined with MeasuresKeyword or  
        /// CriticalDataKeyword.  
        /// </summary>  
        public const EventKeywords TelemetryKeyword = (EventKeywords)0x0000200000000000;

        /// <summary>  
        /// Add MeasuresKeyword to eventSourceOptions.Keywords to indicate that  
        /// an event is for understanding measures and reporting scenarios.  
        /// This keyword should not be combined with TelemetryKeyword or  
        /// CriticalDataKeyword.  
        /// </summary>  
        public const EventKeywords MeasuresKeyword = (EventKeywords)0x0000400000000000;

        /// <summary>  
        /// Add CriticalDataKeyword to eventSourceOptions.Keywords to indicate that  
        /// an event powers user experiences or is critical to business intelligence.  
        /// This keyword should not be combined with TelemetryKeyword or  
        /// MeasuresKeyword.  
        /// </summary>  
        public const EventKeywords CriticalDataKeyword = (EventKeywords)0x0000800000000000;

        /// <summary>  
        /// Add CoreData to eventSourceOptions.Tags to indicate that an event  
        /// contains high priority "core data". (Core data is defined by the telemetry  
        /// team. If you think your data is "core data", please work with the telemetry  
        /// team to add your event to the "core data" list before adding this flag to  
        /// your event.)  
        /// </summary>  
        public const EventTags CoreData = (EventTags)0x00080000;

        /// <summary>  
        /// Add InjectXToken to eventSourceOptions.Tags to indicate that an XBOX  
        /// identity token should be injected into the event before the event is  
        /// uploaded.  
        /// </summary>  
        public const EventTags InjectXToken = (EventTags)0x00100000;

        /// <summary>  
        /// Add RealtimeLatency to eventSourceOptions.Tags to indicate that an event  
        /// should be transmitted in real time (via any available connection).  
        /// </summary>  
        public const EventTags RealtimeLatency = (EventTags)0x0200000;

        /// <summary>  
        /// Add NormalLatency to eventSourceOptions.Tags to indicate that an event  
        /// should be transmitted via the preferred connection based on device policy.  
        /// </summary>  
        public const EventTags NormalLatency = (EventTags)0x0400000;

        /// <summary>  
        /// Add CriticalPersistence to eventSourceOptions.Tags to indicate that an  
        /// event should be deleted last when low on spool space.  
        /// </summary>  
        public const EventTags CriticalPersistence = (EventTags)0x0800000;

        /// <summary>  
        /// Add NormalPersistence to eventSourceOptions.Tags to indicate that an event  
        /// should be deleted first when low on spool space.  
        /// </summary>  
        public const EventTags NormalPersistence = (EventTags)0x1000000;

        /// <summary>  
        /// Add DropPii to eventSourceOptions.Tags to indicate that an event contains  
        /// PII and should be anonymized by the telemetry client. If this tag is  
        /// present, PartA fields that might allow identification or cross-event  
        /// correlation will be removed from the event.  
        /// </summary>  
        public const EventTags DropPii = (EventTags)0x02000000;

        /// <summary>  
        /// Add HashPii to eventSourceOptions.Tags to indicate that an event contains  
        /// PII and should be anonymized by the telemetry client. If this tag is  
        /// present, PartA fields that might allow identification or cross-event  
        /// correlation will be hashed (obfuscated).  
        /// </summary>  
        public const EventTags HashPii = (EventTags)0x04000000;

        /// <summary>  
        /// Add MarkPii to eventSourceOptions.Tags to indicate that an event contains  
        /// PII but may be uploaded as-is. If this tag is present, the event will be  
        /// marked so that it will only appear on the Asimov private stream.  
        /// </summary>  
        public const EventTags MarkPii = (EventTags)0x08000000;

        /// <summary>  
        /// Add DropPiiField to eventFieldAttribute.Tags to indicate that a field  
        /// contains PII and should be dropped by the telemetry client.  
        /// </summary>  
        public const EventFieldTags DropPiiField = (EventFieldTags)0x04000000;

        /// <summary>  
        /// Add HashPiiField to eventFieldAttribute.Tags to indicate that a field  
        /// contains PII and should be hashed (obfuscated) prior to uploading.  
        /// </summary>  
        public const EventFieldTags HashPiiField = (EventFieldTags)0x08000000;

        /// <summary>  
        /// The value that makes an EventSource join the provider telemetry group.  
        /// </summary>  
        private static readonly string[] telemetryTraits = { "ETW_GROUP", "{" + PartnerProviderGuid + "}" };

        /// <summary>  
        /// Constructs a new instance of the PartnerTelemetryEventSource class with the  
        /// specified name. Sets the EtwSelfDescribingEventFormat option and joins the  
        /// MicrosoftTelemetry group.  
        /// </summary>  
        /// <param name="eventSourceName">The name of the event source.</param>  
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811", Justification = "Shared class with tiny helper methods - not all constructors/methods are used by all consumers")]
        public PartnerTelemetryEventSource(string eventSourceName)
            : base(eventSourceName, EventSourceSettings.EtwSelfDescribingEventFormat, telemetryTraits)
        {
            return;
        }

        /// <summary>  
        /// For use by derived classes that set the eventSourceName via EventSourceAttribute.  
        /// Sets the EtwSelfDescribingEventFormat option and joins the  
        /// MicrosoftTelemetry group.  
        /// </summary>  
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811", Justification = "Shared class with tiny helper methods - not all constructors/methods are used by all consumers")]
        protected PartnerTelemetryEventSource()
            : base(EventSourceSettings.EtwSelfDescribingEventFormat, telemetryTraits)
        {
            return;
        }

        public void WriteIntegratationEvent(string hockeyAppIKey)
        {
            this.Write("HockeyAppWatsonIdentification", new EventSourceOptions() { Keywords = TelemetryKeyword }, new HockeyAppIdentity() { iKey = hockeyAppIKey, HockeyAppCorrelationGuid = System.Guid.NewGuid().ToString() });
        }
    }

    /// <summary>
    /// In order for UTC to pick up the payload, properties need to be public.
    /// </summary>
    [EventData]
    internal class HockeyAppIdentity
    {
        public string iKey { get; set; }

        /// <summary>
        /// Gets or sets a GUID in HockeyApp UTC event as well.
        /// This GUID will permit Watson team to join, in the future, if they end up posting directly to Watson from inside the UWP app.
        /// </summary>
        public string HockeyAppCorrelationGuid { get; set; }
    }
}