using HockeyApp.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.ExchangeActiveSyncProvisioning;

namespace HockeyApp
{
    internal partial class HockeyPlatformHelper81
    {

        internal const string Name = "HockeySDKWin81";
        internal const string Version = "2.2.0-beta2";
        internal const string UserAgent = "Hockey/Win81";



        public string OSPlatform
        {
            get { return "Windows"; }
        }

        public string ProductID
        {
            get { return AppxManifest.Current.Package.Identity.Name; }
        }

        public string SDKVersion
        {
            get { return Version; }
        }

        public string SDKName
        {
            get { return Name; }
        }

        public string UserAgentString
        {
            get { return UserAgent; }
        }
    }
}
