namespace Microsoft.HockeyApp.Extensibility
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using DataContracts;
    using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

    using Assert = Xunit.Assert;

    /// <summary>
    /// Component telemetry source tests.
    /// </summary>
    [TestClass]
    public partial class ComponentContextInitializerTest
    {
        [TestMethod]
        public void CallingInitializeOnComponentContextInitializerWithNullThrowsArgumentNullException()
        {
            ComponentContextInitializer source = new ComponentContextInitializer();
            Assert.Throws<AggregateException>(() => source.Initialize(null).Wait());
        }

        [TestMethod]
        public async Task ReadingVersionFromXapManifestYieldsCorrectValue()
        {
            ComponentContextInitializer source = new ComponentContextInitializer();
            var telemetryContext = new TelemetryContext();

            Assert.Null(telemetryContext.Component.Version);

            await source.Initialize(telemetryContext);

#if NET35 || NET40 || NET45
            Assert.Null(telemetryContext.Component.Version);
#else
            Assert.Equal("1.0.0.0", telemetryContext.Component.Version);
#endif
        }
    }
}
