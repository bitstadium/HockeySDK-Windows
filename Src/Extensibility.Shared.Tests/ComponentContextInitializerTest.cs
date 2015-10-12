namespace Microsoft.HockeyApp.Extensibility
{
    using System;
    using System.Reflection;
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
        public void ComponentContextInitializerClassIsPublicToEnableInstantiation()
        {
            Assert.True(typeof(ComponentContextInitializer).GetTypeInfo().IsPublic);
        }

        [TestMethod]
        public void CallingInitializeOnComponentContextInitializerWithNullThrowsArgumentNullException()
        {
            ComponentContextInitializer source = new ComponentContextInitializer();
            Assert.Throws<ArgumentNullException>(() => source.Initialize(null));
        }

        [TestMethod]
        public void ReadingVersionFromXapManifestYieldsCorrectValue()
        {
            ComponentContextInitializer source = new ComponentContextInitializer();
            var telemetryContext = new TelemetryContext();

            Assert.Null(telemetryContext.Component.Version);

            source.Initialize(telemetryContext);

#if NET35 || NET40 || NET45
            Assert.Null(telemetryContext.Component.Version);
#else
            Assert.Equal("1.0.0.0", telemetryContext.Component.Version);
#endif
        }
    }
}
