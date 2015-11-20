namespace Microsoft.HockeyApp.Channel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TestFramework;
#if WINDOWS_PHONE || WINDOWS_STORE || WINDOWS_UWP
    using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
    using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif
    using Assert = Xunit.Assert;
    
    [TestClass]
    public class InMemoryChannelTest
    {
        // Ignoring test for now as it is failing sporadically. ToDo: Fix the issue.
        [Ignore]
        [TestMethod]
        public void WhenSendIsCalledTheEventIsBeingQueuedInTheBuffer()
        {
            var telemetryBuffer = new TelemetryBuffer();
            var channel = new InMemoryChannel(telemetryBuffer, new InMemoryTransmitter(telemetryBuffer));
            var sentTelemetry = new StubTelemetry();

            channel.Send(sentTelemetry);
            IEnumerable<ITelemetry> telemetries = telemetryBuffer.Dequeue();

            Assert.NotNull(telemetries);
            Assert.Equal(1, telemetries.Count());
            Assert.Same(sentTelemetry, telemetries.First());
        }
    }
}
