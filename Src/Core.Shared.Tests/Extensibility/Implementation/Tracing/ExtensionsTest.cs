namespace Microsoft.HockeyApp.Extensibility.Implementation.Tracing
{
    using System;
    using System.Globalization;
    using System.Threading;
    using TestFramework;
#if WINDOWS_PHONE || WINDOWS_STORE || WINDOWS_UWP
    using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
    using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif
    using Assert = Xunit.Assert;

    public class ExtensionsTest
    {
        [TestClass]
        public class ToInvariantString
        {
#if !WINRT && !WINDOWS_UWP
            private CultureInfo originalUICulture = Thread.CurrentThread.CurrentUICulture;

            [TestCleanup]
            public void Cleanup()
            {
                Thread.CurrentThread.CurrentUICulture = this.originalUICulture;
            }

            [TestMethod]
            public void ExtractsStackTraceWithInvariantCultureToHelpOurTelemetryToolsMatchSimilarErrorsReportedByOSsWithDifferentLanguages()
            {
                CultureInfo stackTraceCulture = null;
                var exception = new StubException();
                exception.OnToString = () =>
                {
                    stackTraceCulture = Thread.CurrentThread.CurrentUICulture;
                    return string.Empty;
                };

                DiagnoisticsEventThrottlingScheduler.ConvertExceptionToInvariantString(exception);

                Assert.Same(CultureInfo.InvariantCulture, stackTraceCulture);
            }

            [TestMethod]
            public void RestoresOriginalUICultureToPreserveGlobalStateOfApplication()
            {
                DiagnoisticsEventThrottlingScheduler.ConvertExceptionToInvariantString(new Exception());
                Assert.Same(this.originalUICulture, Thread.CurrentThread.CurrentUICulture);
            }
#endif
        }
    }
}
