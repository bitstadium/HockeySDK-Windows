namespace Microsoft.ApplicationInsights.Channel
{
    using System;
#if WINDOWS_PHONE || WINDOWS_PHONE_APP || WINDOWS_STORE
    using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
    using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

    public class UniversalTelemetryChannelTest : AsyncTest
    {
        [TestClass]
        public class DeveloperMode : UniversalTelemetryChannelTest
        {
            [TestMethod]
            public void DeveloperModeIsFalseByDefault()
            {
                var channel = new UniversalTelemetryChannel();
                Assert.IsTrue(channel.DeveloperMode == false);
            }

            [TestMethod]
            public void DeveloperModeCanBeModifiedByConfiguration()
            {
                var channel = new UniversalTelemetryChannel();
                channel.DeveloperMode = true;
                Assert.IsTrue(channel.DeveloperMode == true);
            }
        }

        [TestClass]
        public class Send : UniversalTelemetryChannelTest
        {
            [TestMethod]            
            public void ThrowsArgumentNullExceptionWhenTelemetryIsNullToPreventDeveloperErrors()
            {
                using (var channel = new UniversalTelemetryChannel())
                {
                    Exception exp = null;
                    try
                    {
                        channel.Send(null);
                    }
                    catch (Exception exception)
                    {
                        exp = exception;                        
                    }

                    Assert.IsNotNull(exp);
                    Assert.IsInstanceOfType(exp, typeof(ArgumentNullException));
                }
            }
        }
    }
}