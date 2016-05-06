namespace Microsoft.HockeyApp.Extensibility.Implementation.Tracing
{
    using System;
    using System.Collections.Generic;
#if CORE_PCL || NET45 || NET46 || WINRT || WINDOWS_UWP
    using System.Diagnostics.Tracing;
#endif
#if NET35 || NET40
    using Microsoft.Diagnostics.Tracing;
#endif
#if WINDOWS_PHONE || WINDOWS_STORE || WINDOWS_UWP
    using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
    using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif
    using Tracing;
    using Tracing.Mocks;

    [TestClass]
    public class DiagnosticsListenerTest
    {
        [TestMethod]
        public void TestConstructorThrowsArgumentException()
        {
            bool failedWithExpectedException = false;
            try
            {
                using (var listener = new DiagnosticsListener(null))
                {
                    // nop
                }
            }
            catch (ArgumentNullException)
            {
                failedWithExpectedException = true;
            }

            Assert.IsTrue(failedWithExpectedException);
        }

        [TestMethod]
        public void TestEventSending()
        {
            var senderMock = new F5DiagnosticsSenderMock();
            var senders = new List<IDiagnosticsSender> { senderMock };
            using (var listener = new DiagnosticsListener(senders))
            {
#if SILVERLIGHT
                CoreEventSource.Log.EnableEventListener(listener);
#endif
                listener.LogLevel = EventLevel.Verbose;
                CoreEventSource.Log.LogVerbose("failure");
            }

            Assert.AreEqual(1, senderMock.Messages.Count);
            Assert.AreEqual("[msg=Log verbose];[msg=failure]", senderMock.Messages[0]);
        }
    }
}
