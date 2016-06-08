using Microsoft.HockeyApp.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.HockeyApp
{
    internal partial class HockeyPlatformHelper81
    {

        internal const string Name = "HockeySDKWP81";
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
            get { return Extensibility.SdkVersionPropertyContextInitializer.GetAssemblyVersion(); }
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
