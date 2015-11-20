namespace Microsoft.HockeyApp.Extensibility.Tests
{
    using System;
    using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
    using Microsoft.HockeyApp.Extensibility;

    [TestClass]
    public class DeviceContextReaderTest
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
                Assert.IsFalse(DeviceContextReader.IsNativeEnvironment(ex));
            }
        }
    }
}
