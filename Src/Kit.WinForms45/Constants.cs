namespace Microsoft.HockeyApp
{
    using System;
    using System.IO;

    internal class HockeyConstants
    {

        public static string GetPathToHockeyCrashes()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (!path.EndsWith("\\", StringComparison.OrdinalIgnoreCase)) { path += "\\"; }
            path += "HockeyCrashes\\";
            if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }
            return path;
        }

        internal const string CrashFilePrefix = "crashinfo_";

        internal const string USER_AGENT_STRING = "Hockey/WinForms";
        internal const string SDKNAME = "HockeySDKWinForms";

        // ToDo: Read version from assembly version attribute.
        internal const string SDKVERSION = "2.2.0-beta5"; 

        internal const string NAME_OF_SYSTEM_SEMAPHORE = "HOCKEYAPPSDK_SEMAPHORE";
    }
}
