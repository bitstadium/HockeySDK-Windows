namespace Microsoft.HockeyApp
{
    using System;
    using System.IO;

    internal class HockeyConstants
    {
        internal const string CrashFilePrefix = "crashinfo_";
        internal const string USER_AGENT_STRING = "Hockey/WinWPF";
        internal const string SDKNAME = "HockeySDKWinWPF";
        internal const string NAME_OF_SYSTEM_SEMAPHORE = "HOCKEYAPPSDK_SEMAPHORE";

        internal static string GetPathToHockeyCrashes()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (!path.EndsWith("\\", StringComparison.OrdinalIgnoreCase)) { path += "\\"; }
            path += "HockeyApp\\" + HockeyClientWPFExtensions.AppUniqueFolderName + "\\";
            if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }
            return path;
        }
    }
}
