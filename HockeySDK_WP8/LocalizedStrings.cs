using HockeyApp.Resources;
using System.Reflection;

namespace HockeyApp
{
    /// <summary>
    /// Provides access to string resources.
    /// </summary>
    public class LocalizedStrings
    {
        

        private static SdkResources _localizedResources = new SdkResources();
        
        public SdkResources LocalizedResources { get { 
            
//            Assembly.GetExecutingAssembly()
            
            return _localizedResources; } }
    }
}