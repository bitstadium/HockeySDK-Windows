namespace Microsoft.HockeyApp.Extensibility.Implementation.Platform
{
    using System;
    using System.IO;
    using System.Text;
#if WINDOWS_PHONE_APP || WINDOWS_PHONE || WINDOWS_STORE || WINDOWS_UWP
    using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
    using global::Windows.ApplicationModel;
    using global::Windows.Storage;
    using Services;
#else
    using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

    /// <summary>
    /// Shared, platform-neutral tests for <see cref="PlatformImplementation"/> class.
    /// </summary>
    [TestClass]
    public partial class PlatformImplementationTest : IDisposable
    {
        public PlatformImplementationTest()
        {
            // Make sure configuration files created by other tests don't brake these.
            DeleteConfigurationFile();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        [TestMethod]
        public void ReadConfigurationXmlIgnoresMissingConfigurationFileByReturningEmptyString()
        {
            var platform = ServiceLocator.GetService<IPlatformService>();
            string configuration = platform.ReadConfigurationXml();
            Assert.AreEqual(0, configuration.Length);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing == true)
            {
                DeleteConfigurationFile();
            }
        }

        private static void CreateConfigurationFile(string content)
        {
            using (Stream fileStream = OpenConfigurationFile())
            {
                byte[] configurationBytes = Encoding.UTF8.GetBytes(content);
                fileStream.Write(configurationBytes, 0, configurationBytes.Length);
            }           
        }

        private static void DeleteConfigurationFile()
        {
#if WINRT || WINDOWS_UWP
            try 
            {
                StorageFile file = Package.Current.InstalledLocation.GetFileAsync(PlatformTest.ConfigurationFileName).GetAwaiter().GetResult();
                file.DeleteAsync().GetAwaiter().GetResult();
            }
            catch (FileNotFoundException)
            {
            }
#elif WINDOWS_PHONE_APP
            File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PlatformTest.ConfigurationFileName));
#else
            File.Delete(Path.Combine(Environment.CurrentDirectory, PlatformTest.ConfigurationFileName));
#endif
        }

        private static Stream OpenConfigurationFile()
        {
#if WINRT || WINDOWS_UWP
            StorageFile file = Package.Current.InstalledLocation.CreateFileAsync(PlatformTest.ConfigurationFileName, CreationCollisionOption.ReplaceExisting).GetAwaiter().GetResult();
            return file.OpenStreamForWriteAsync().GetAwaiter().GetResult();
#elif WINDOWS_PHONE_APP
            return File.OpenWrite(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PlatformTest.ConfigurationFileName));
#else
            return File.OpenWrite(Path.Combine(Environment.CurrentDirectory, PlatformTest.ConfigurationFileName));
#endif
        }
    }
}
