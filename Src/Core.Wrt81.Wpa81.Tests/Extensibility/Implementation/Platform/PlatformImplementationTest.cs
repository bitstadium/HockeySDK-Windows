namespace Microsoft.HockeyApp.Extensibility.Implementation.Platform
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Channel;
    using Extensibility.Implementation;
    using TestFramework;
    using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
    using Windows.ApplicationModel;
    using Windows.Storage;
    using Assert = Xunit.Assert;

    /// <summary>
    /// Windows Runtime tests for the <see cref="PlatformImplementation"/> class.
    /// </summary>
    public partial class PlatformImplementationTest
    {
        [TestMethod]
        public void GetApplicationSettingsReturnsDictionaryFromLocalSettingsOfApplicationData()
        {
            string testKey = Guid.NewGuid().ToString();
            string testValue = Guid.NewGuid().ToString();
            ApplicationData.Current.LocalSettings.Values[testKey] = testValue;
            try
            {
                var platform = new PlatformImplementation();
                IDictionary<string, object> settings = platform.GetLocalApplicationSettings();

                // Can't use object reference equality here because ApplicationData.Current.LocalSettings.Values returns a new instance every time
                Assert.Equal(testValue, settings[testKey]);
            }
            finally
            {
                ApplicationData.Current.LocalSettings.Values.Remove(testKey);
            }
        }
    }
}
