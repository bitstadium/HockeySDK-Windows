using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HockeyApp
{
    public class Constants
    {

        public static string GetPathToHockeyCrashes()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (!path.EndsWith("\\")) { path += "\\"; }
            path += "HockeyCrashes\\";
            if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }
            return path;
        }

        internal const string USER_AGENT_STRING = "Hockey/NET40/WPF";
        internal const string SDKNAME = "HockeySDK.Net40.WPF";
        internal const string SDKVERSION = "1.0";
    }
}
