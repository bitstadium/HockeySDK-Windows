namespace Microsoft.HockeyApp.Extensibility.Windows
{
    using System;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
#if SILVERLIGHT
    using System.Windows;
#endif
    using Channel;
    using DataContracts;
    using Extensibility.Implementation.Platform;
    using TestFramework;
    using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#if WINRT || WINDOWS_UWP
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

        [Ignore]
        [TestMethod]
        public void ClassIsPublicToAllowUsersConfigureItProgrammatically()
        {
            Assert.True(typeof(UnhandledExceptionTelemetryModule).GetTypeInfo().IsPublic);
        }
    }
}
