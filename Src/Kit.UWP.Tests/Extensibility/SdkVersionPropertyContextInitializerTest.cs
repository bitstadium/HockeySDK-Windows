namespace Microsoft.HockeyApp.Extensibility
{
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using DataContracts;
#if WINDOWS_PHONE || WINDOWS_PHONE_APP || WINDOWS_STORE || WINDOWS_UWP
    using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
    using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif
    using Assert = Xunit.Assert;
    using Services;
    [TestClass]
    public class SdkVersionPropertyContextInitializerTest
    {
        [TestMethod]
        public void ClassIsInternalAndNotMeantToBeUsedByCustomers()
        {
            Assert.False(typeof(SdkVersionPropertyContextInitializer).GetTypeInfo().IsPublic);
        }

        [TestMethod]
        public void ClassImplementsIContextInitializerToSupportTelemetryContext()
        {
            Assert.True(typeof(IContextInitializer).GetTypeInfo().IsAssignableFrom(typeof(SdkVersionPropertyContextInitializer).GetTypeInfo()));
        }

        [TestMethod]
        public async Task InitializeSetsSdkVersionPropertyOfGivenTelemetry()
        {
            ServiceLocator.AddService<IPlatformService>(new PlatformService());
            var initializer = new SdkVersionPropertyContextInitializer();
            var telemetryContext = new TelemetryContext();
            await initializer.Initialize(telemetryContext);

            Assert.NotNull(telemetryContext.Internal.SdkVersion);
        }

        [TestMethod]
        public async Task InitializeSetsSdkVersionValueAsAssemblyVersion()
        {
            ServiceLocator.AddService<IPlatformService>(new PlatformService());
            var initializer = new SdkVersionPropertyContextInitializer();
            var telemetryContext = new TelemetryContext();
            await initializer.Initialize(telemetryContext);
            
            string expectedSdkVersion;
            expectedSdkVersion = typeof(SdkVersionPropertyContextInitializer).GetTypeInfo().Assembly.GetCustomAttributes<AssemblyFileVersionAttribute>()
                    .First()
                    .Version;
            Assert.Equal("hockeysdk.uwp:" + expectedSdkVersion, telemetryContext.Internal.SdkVersion);
        }
    }
}
