namespace Microsoft.ApplicationInsights.Extensibility.Windows
{
    using System;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
#if SILVERLIGHT
    using System.Windows;
#endif
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility.Implementation.Platform;
    using Microsoft.ApplicationInsights.TestFramework;
    using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#if WINRT
    using global::Windows.UI.Xaml;
#endif
    using Assert = Xunit.Assert;

    [TestClass]
    public sealed class UnhandledExceptionTelemetryModuleTest : IDisposable
    {
        private readonly ManualResetEventSlim telemetrySent = new ManualResetEventSlim();
        private readonly StubTelemetryChannel channel = new StubTelemetryChannel();
        private ITelemetry sentTelemetry = null;       

        public UnhandledExceptionTelemetryModuleTest()
        {
            this.channel.OnSend = telemetry =>
            {
                sentTelemetry = telemetry;
                telemetrySent.Set();
            };
        }

        public void Dispose()
        {
            this.telemetrySent.Dispose();
            this.channel.Dispose();
        }

        [TestMethod]
        public void ClassIsPublicToAllowUsersConfigureItProgrammatically()
        {
            Assert.True(typeof(UnhandledExceptionTelemetryModule).GetTypeInfo().IsPublic);
        }
        
        [TestMethod]
        public async Task InitializeInitializesModuleAsynchronously()
        {
            using (var module = new UnhandledExceptionTelemetryModule())
            {
                module.Initialize(new TelemetryConfiguration() { TelemetryChannel = new StubTelemetryChannel() });
                await module.Initialized;
            }
        }
    }
}
