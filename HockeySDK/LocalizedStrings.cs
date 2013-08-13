using HockeyApp.Resources;

namespace HockeyApp
{
    /// <summary>
    /// Provides access to string resources.
    /// </summary>
    public class LocalizedStrings
    {
        private static SdkResources _localizedResources = new SdkResources();

        public SdkResources LocalizedResources { get { return _localizedResources; } }
    }
}