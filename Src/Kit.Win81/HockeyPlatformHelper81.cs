namespace Microsoft.HockeyApp
{
    using Microsoft.HockeyApp.Tools;

    internal partial class HockeyPlatformHelper81
    {
        internal const string Name = "HockeySDKWin81";
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
