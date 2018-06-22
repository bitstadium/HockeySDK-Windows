namespace Microsoft.HockeyApp.Extensibility
{
    using System;
    using System.Reflection;

    using Channel;
    using Extensibility;
    using TestFramework;
#if WINDOWS_PHONE || WINDOWS_STORE || WINDOWS_UWP
    using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
    using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif
    using Assert = Xunit.Assert;

    [TestClass]
    public class TimestampPropertyInitializerTest
    {
        [TestMethod]
        public void ClassImplementsITelemetryInitializerToSupportTelemetryContext()
        {
            Assert.True(typeof(ITelemetryInitializer).GetTypeInfo().IsAssignableFrom(typeof(TimestampPropertyInitializer).GetTypeInfo()));
        }

        [TestMethod]
        public void InitializeSetsTimestampPropertyOfGivenTelemetry()
        {
            var initializer = new TimestampPropertyInitializer();
            var telemetry = new StubTelemetry();
            initializer.Initialize(telemetry);
            Assert.True(DateTimeOffset.Now.Subtract(telemetry.Timestamp) < TimeSpan.FromMinutes(1));
        }

        [TestMethod]
        public void InitializeDoesNotOverrideTimestampSpecifiedExplicitly()
        {
            var initializer = new TimestampPropertyInitializer();
            var expected = DateTimeOffset.UtcNow;
            var telemetry = new StubTelemetry { Timestamp = expected };
            initializer.Initialize(telemetry);
            Assert.Equal(expected, telemetry.Timestamp);
        }
    }
}
