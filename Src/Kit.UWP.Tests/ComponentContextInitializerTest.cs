namespace Microsoft.HockeyApp.Extensibility
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using DataContracts;
    using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

    using Assert = Xunit.Assert;
    using Services;
    using Services.Device;    /// <summary>
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
    }
}
