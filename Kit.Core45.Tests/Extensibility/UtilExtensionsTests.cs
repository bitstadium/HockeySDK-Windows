using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.HockeyApp;

namespace Kit.Core45.Tests
{
    [TestClass]
    public class UtilExtensionsTests
    {
        #region ToLongUriEscapeDataString

        [TestMethod]
        public void StringsLongerThen32KSuccessfullyEncoded()
        {
            string str = new String('z', 33000) + " ";
            var encodedStr = str.ToLongUriEscapeDataString();
            Assert.IsTrue(encodedStr.EndsWith("%20"));

            int expectedLength = 33000 + "%20".Length;
            Assert.AreEqual(encodedStr.Length, expectedLength);
        }

        [TestMethod]
        public void StringsShorterThen32KSuccessfullyEncoded()
        {
            string str = new String('z', 1000) + " ";
            var encodedStr = str.ToLongUriEscapeDataString();
            Assert.IsTrue(encodedStr.EndsWith("%20"));

            int expectedLength = 1000 + "%20".Length;
            Assert.AreEqual(encodedStr.Length, expectedLength);
        }

        [TestMethod]
        public void emptyStringEncodingDoesNothing()
        {   
            Assert.AreEqual(String.Empty.ToLongUriEscapeDataString(), String.Empty);
        }

        [TestMethod]
        public void StringsEqualToLimtSuccessfullyEncoded()
        {
            string str = new String('z', 32765) + " ";
            var encodedStr = str.ToLongUriEscapeDataString();
            Assert.IsTrue(encodedStr.EndsWith("%20"));

            int expectedLength = 32765 + "%20".Length;
            Assert.AreEqual(encodedStr.Length, expectedLength);
        }

        #endregion
    }
}
