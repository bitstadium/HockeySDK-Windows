namespace Microsoft.ApplicationInsights.Channel
{
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Channel;    
#if WINDOWS_STORE
    using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
    using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif
    
    [TestClass]
    public class EventSourceWriterTest
    {
        [TestMethod]
        public void ProviderIdAndNameIsCorrectAndTheSameForAllCasingsOfInstrumentationKey()
        {
            Guid testGuid = Guid.NewGuid();

            using (EventSourceWriter writerUpper = new EventSourceWriter(testGuid.ToString().ToUpperInvariant()))
            using (EventSourceWriter writerLower = new EventSourceWriter(testGuid.ToString().ToLowerInvariant()))
            {
                Assert.AreEqual("Microsoft.ApplicationInsights." + testGuid.ToString("N").ToLowerInvariant(), writerUpper.ProviderName);
                Assert.AreEqual(writerLower.ProviderId, writerUpper.ProviderId);
                Assert.AreEqual(writerLower.ProviderName, writerUpper.ProviderName);
            }
        }

        [TestMethod]
        public void ProviderNameDoesNotContainCharactersOtherThanAlphaNumericOrDot()
        {
            Guid testGuid = Guid.NewGuid();

            string expectedName = "Microsoft.ApplicationInsights.aic" + testGuid.ToString("N").ToLowerInvariant();

            using (EventSourceWriter writer = new EventSourceWriter("AIC:" + testGuid.ToString()))
            {
                MatchCollection matches = Regex.Matches(writer.ProviderName, "(?:[^a-z0-9.])", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                Assert.AreEqual(0, matches.Count);
                Assert.AreEqual(expectedName, writer.ProviderName);
            }
        }

        /// <summary>
        /// <see href="http://osgwiki/wiki/Structured_iKey">Structured iKey</see> prefix must be upper-case.
        /// </summary>
        [TestMethod]
        public void StructuredInstrumentationKeyPrefixCasingIsPreserved()
        {
            string prefix = "AIC";
            string testKey = prefix + "-" + Guid.NewGuid().ToString();
            string expected = testKey.Substring(0, prefix.Length);

            using (EventSourceWriter writer = new EventSourceWriter(testKey))
            {
                Assert.AreEqual(testKey, writer.InstrumentationKey);
                Assert.AreEqual(expected, writer.InstrumentationKey.Substring(0, prefix.Length));
            }
        }

        [TestMethod]
        public void FullEventNameCanBeSeperatedIntoParts()
        {
            Guid testGuid = Guid.NewGuid();

            string expectedProviderName = "Microsoft.ApplicationInsights";
            string expectedInstrumentationKey = "aic" + testGuid.ToString("N").ToLowerInvariant();
            string expectedEventName = "TestEvent";

            using (EventSourceWriter writer = new EventSourceWriter("AIC:" + testGuid.ToString()))
            {
                string fullEventName = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", writer.ProviderName, expectedEventName);

                int indexOfEventName = fullEventName.LastIndexOf('.');
                Assert.AreEqual(expectedEventName, fullEventName.Substring(indexOfEventName + 1));

                int indexOfInstrumentationKey = fullEventName.LastIndexOf('.', indexOfEventName - 1);
                Assert.AreEqual(expectedInstrumentationKey, fullEventName.Substring(indexOfInstrumentationKey + 1, indexOfEventName - indexOfInstrumentationKey - 1));

                Assert.AreEqual(expectedProviderName, fullEventName.Substring(0, indexOfInstrumentationKey));
            }
        }
    }
}