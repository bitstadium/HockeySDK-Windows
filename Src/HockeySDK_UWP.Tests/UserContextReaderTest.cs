namespace Microsoft.HockeyApp.Extensibility.Tests
{
    using System;
    using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
    using Microsoft.HockeyApp.Extensibility;

    [TestClass]
    public class UserContextReaderTest
    {
        [TestMethod]
        public void TestIsNativeEnvironment()
        {
            try
            {
                throw new Exception();
            }
            catch (Exception ex)
            {
                Assert.IsFalse(UserContextReader.IsNativeEnvironment(ex));
            }
        }
    }
}
