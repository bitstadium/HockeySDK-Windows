using HockeyApp.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HockeyApp
{
    internal partial class HockeyPlatformHelper81
    {

        internal const string Name = "HockeySDKWP81";
        internal const string Version = "2.2.0-beta1";
        internal const string UserAgent = "Hockey/WP81";

        public string OSPlatform
        {
            get { return "Windows Phone"; }
        }

        public string ProductID
        {
            get { return AppxManifest.Current.Package.PhoneIdentity.PhoneProductId; }
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
