namespace Microsoft.HockeyApp
{
    internal class HockeyConstants
    {
        internal const string USER_AGENT_STRING = "Hockey/WinForms";
        internal const string SDKNAME = "HockeySDKWinForms";

        internal static string SDKVERSION
        {
            get
            {
                return Extensibility.SdkVersionPropertyContextInitializer.GetAssemblyVersion();
            }
        }
    }
}
