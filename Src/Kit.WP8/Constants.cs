namespace Microsoft.HockeyApp
{
    internal class Constants
    {
        internal const string CrashDirectoryName = "HockeyCrashes";
        internal const string OldCrashDirectoryName = "CrashLogs";
        internal const string CrashFilePrefix = "crashinfo_";
        internal const string SdkName = "HockeySDKWP8";

        internal const string UserAgentString = "Hockey/WP8";

        internal const string FeedbackThreadKey = "HockeyAppFeedback_ThreadId";
        internal const string FeedbackEmailKey = "HockeyAppFeedback_Email";
        internal const string FeedbackUsernameKey = "HockeyAppFeedback_Username";
        internal const string FeedbackThreadSubjectKey = "HockeyAppFeedback_Subject";

        internal const string AuthLastAuthorizedVersionKey = "HockeyAppAuth_LastVersionAuthorized";
        internal const string AuthStatusKey = "HockeyAppAuth_Status";

        internal const string ContentTypeUrlEncoded = "application/x-www-form-urlencoded"; 

        internal static string SdkVersion
        {
            get
            {
                return Extensibility.SdkVersionPropertyContextInitializer.GetAssemblyVersion();
            }
        }
    }
}
