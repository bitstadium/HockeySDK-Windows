namespace Microsoft.HockeyApp
{
    internal class Constants
    {
        internal const string TelemetryServiceEndpoint = "https://gate.hockeyapp.net/v2/track";

        internal const string TelemetryNamePrefix = "Microsoft.ApplicationInsights.";

        internal const string DevModeTelemetryNamePrefix = "Microsoft.ApplicationInsights.Dev.";

        // This is a special EventSource key for groups and cannot be changed.
        internal const string EventSourceGroupTraitKey = "ETW_GROUP";

        internal const int MaxExceptionCountToSave = 10;
    }
}
