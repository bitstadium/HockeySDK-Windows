namespace Microsoft.ApplicationInsights.Channel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
#if NET40 || NET45 || NET35
    using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
    using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif
    using Assert = Xunit.Assert;
#if !NET35
    using EnvironmentEx = System.Environment;    
#endif
    using TaskEx = System.Threading.Tasks.Task;
    using Microsoft.HockeyApp.Channel;

    public class TelemetryBufferTest
    {
        [TestClass]
        public class MaxNumberOfItemsPerTransmission : TelemetryBufferTest
        {
            [TestMethod]
            public void DefaultValueIsAppropriateForProductionEnvironmentAndUnitTests()
            {
                var buffer = new TelemetryBuffer();
                Assert.Equal(500, buffer.Capacity);
            }

            [TestMethod]
            public void CanBeSetByChannelToTunePerformance()
            {
                var buffer = new TelemetryBuffer();
                buffer.Capacity = 42;
                Assert.Equal(42, buffer.Capacity);
            }
        }
    }
}
