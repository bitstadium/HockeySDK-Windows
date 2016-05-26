namespace Microsoft.HockeyApp
{
    internal class SDKConstants
    {
        internal const string SdkName = "HockeySDKWinPCL";
        internal const string PublicApiDomain = "https://rink.hockeyapp.net";
        internal const string UserAgentString = "Hockey/WinPCL";
        internal const string CrashDirectoryName = "HockeyCrashes";
        internal const string CrashFilePrefix = "crashinfo_";

        internal static string SdkVersion
        {
            get
            {
                return Extensibility.SdkVersionPropertyContextInitializer.GetAssemblyVersion();
            }
        }
    }
}
