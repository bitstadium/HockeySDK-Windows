using Microsoft.HockeyApp.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Microsoft.HockeyApp
{

#pragma warning disable 1998

    /// <summary>
    /// HockeyPlatformHelperWPF class.
    /// </summary>
    internal class HockeyPlatformHelperWPF : IHockeyPlatformHelper
    {
        private const string FILE_PREFIX = "HA__SETTING_";
        IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);

        private ApplicationService applicationService;
        private DeviceService deviceService;

        /// <summary>
        /// Initializes a new instance of the <see cref="HockeyPlatformHelperWPF"/> class.
        /// </summary>
        /// <param name="applicationService">The application service.</param>
        /// <param name="deviceService">The device service.</param>
        internal HockeyPlatformHelperWPF(ApplicationService applicationService, DeviceService deviceService)
        {
            this.applicationService = applicationService;
            this.deviceService = deviceService;
        }

        private string PostfixWithAppIdHash(string folderName, bool noDirectorySeparator = false)
        {
            return ((folderName ?? "") + (noDirectorySeparator ? "" : "" + Path.DirectorySeparatorChar) + HockeyClientWPFExtensions.AppIdHash);
        }

        /// <summary>
        /// Sets the setting value.
        /// </summary>
        /// <param name="key">Key value.</param>
        /// <param name="value">Value value.</param>
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "ToDo: Fix it later.")]
        public void SetSettingValue(string key, string value)
        {
            using (var fileStream = isoStore.OpenFile(PostfixWithAppIdHash(FILE_PREFIX + key, true), FileMode.Create, FileAccess.Write))
            {
                using (var writer = new StreamWriter(fileStream))
                {
                    writer.Write(value);
                }
            }
        }

        /// <summary>
        /// Gets setting value.
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "ToDo: Fix it later.")]
        public string GetSettingValue(string key)
        {
            if (isoStore.FileExists(FILE_PREFIX + key))
            {
                using (var fileStream = isoStore.OpenFile(PostfixWithAppIdHash(FILE_PREFIX + key, true), FileMode.Open, FileAccess.Read))
                {
                    using (var reader = new StreamReader(fileStream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Remove Setting value.
        /// </summary>
        /// <param name="key"></param>
        public void RemoveSettingValue(string key)
        {
            if (isoStore.FileExists(PostfixWithAppIdHash(FILE_PREFIX + key, true)))
            {
                isoStore.DeleteFile(PostfixWithAppIdHash(FILE_PREFIX + key, true));
            }
        }


        #region File access

        /// <summary>
        /// Deletes file async.
        /// </summary>
        /// <param name="fileName">File name.</param>
        /// <param name="folderName">Folder name.</param>
        /// <returns>True if succeeds, otherwise false.</returns>
        public async Task<bool> DeleteFileAsync(string fileName, string folderName = null)
        {
            if (isoStore.FileExists(PostfixWithAppIdHash(folderName) + Path.DirectorySeparatorChar + fileName))
            {
                isoStore.DeleteFile(PostfixWithAppIdHash(folderName) + Path.DirectorySeparatorChar + fileName);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Tests whether the file exists.
        /// </summary>
        /// <param name="fileName">File name.</param>
        /// <param name="folderName">Folder name.</param>
        /// <returns>True if file exists, otherwise false.</returns>
        public async Task<bool> FileExistsAsync(string fileName, string folderName = null)
        {
            return isoStore.FileExists(PostfixWithAppIdHash(folderName) + Path.DirectorySeparatorChar + fileName);
        }

        /// <summary>
        /// Gets stream.
        /// </summary>
        /// <param name="fileName">File name.</param>
        /// <param name="folderName">Folder name.</param>
        /// <returns>Stream object.</returns>
        public async Task<Stream> GetStreamAsync(string fileName, string folderName = null)
        {
            return isoStore.OpenFile(PostfixWithAppIdHash(folderName) + Path.DirectorySeparatorChar + fileName, FileMode.Open, FileAccess.Read);
        }

        /// <summary>
        /// Writs stream to a file.
        /// </summary>
        /// <param name="dataStream">Data stream.</param>
        /// <param name="fileName">File name.</param>
        /// <param name="folderName">Folder name.</param>
        /// <returns>Task object.</returns>
        public async Task WriteStreamToFileAsync(Stream dataStream, string fileName, string folderName = null)
        {
            // Ensure crashes folder exists
            if (!isoStore.DirectoryExists(PostfixWithAppIdHash(folderName)))
            {
                isoStore.CreateDirectory(PostfixWithAppIdHash(folderName));
            }

            using (var fileStream = isoStore.OpenFile(PostfixWithAppIdHash(folderName) + Path.DirectorySeparatorChar + fileName, FileMode.Create, FileAccess.Write))
            {
                await dataStream.CopyToAsync(fileStream);
            }
        }

        /// <summary>
        /// Gets file name.
        /// </summary>
        /// <param name="folderName">Folder name.</param>
        /// <param name="fileNamePattern">File name pattern.</param>
        /// <returns>Task list.</returns>
        public async Task<IEnumerable<string>> GetFileNamesAsync(string folderName = null, string fileNamePattern = null)
        {
            try
            {
                return isoStore.GetFileNames(PostfixWithAppIdHash(folderName) + Path.DirectorySeparatorChar + fileNamePattern ?? "*");
            }
            catch (DirectoryNotFoundException)
            {
                return new string[0];
            }
        }

        /// <summary>
        /// Gets a value indicating whether platform supports sync writes.
        /// </summary>
        public bool PlatformSupportsSyncWrite
        {
            get { return true; }
        }

        /// <summary>
        /// Writes stream to a file.
        /// </summary>
        /// <param name="dataStream">Data stream.</param>
        /// <param name="fileName">File name.</param>
        /// <param name="folderName">Folder name.</param>
        public void WriteStreamToFileSync(Stream dataStream, string fileName, string folderName = null)
        {
            // Ensure crashes folder exists
            if (!isoStore.DirectoryExists(PostfixWithAppIdHash(folderName)))
            {
                isoStore.CreateDirectory(PostfixWithAppIdHash(folderName));
            }

            using (var fileStream = isoStore.OpenFile(PostfixWithAppIdHash(folderName) + Path.DirectorySeparatorChar + fileName, FileMode.Create, FileAccess.Write))
            {
                dataStream.CopyTo(fileStream);
            }
        }

        #endregion

        /// <summary>
        /// Gets or sets application package name.
        /// </summary>
        public string AppPackageName
        {
            get { return this.applicationService.GetApplicationId(); }
        }

        /// <summary>
        /// Gets or sets application version.
        /// </summary>
        public string AppVersion
        {
            get { return this.applicationService.GetVersion(); }
        }


        /// <summary>
        /// Gets OS version.
        /// </summary>
        public string OSVersion
        {
            get { return this.deviceService.GetOperatingSystemVersion(); }
        }

        /// <summary>
        /// Gets OS platform name.
        /// </summary>
        public string OSPlatform
        {
            get { return "Windows"; }
        }


        /// <summary>
        /// Gets SDK version.
        /// </summary>
        public string SDKVersion
        {
            get { return Extensibility.SdkVersionPropertyContextInitializer.GetAssemblyVersion(); }
        }


        /// <summary>
        /// Gets SDK name.
        /// </summary>
        public string SDKName
        {
            get { return HockeyConstants.SDKNAME; }
        }

        /// <summary>
        /// Gets user agent.
        /// </summary>
        public string UserAgentString
        {
            get { return HockeyConstants.USER_AGENT_STRING; }
        }

        private string _productID = null;

        /// <summary>
        /// Gets or sets product ID.
        /// </summary>
        public string ProductID
        {
            get { return _productID; }
            set { _productID = value; }
        }


        /// <summary>
        /// Gets manufacturer.
        /// </summary>
        public string Manufacturer
        {
            get { return deviceService.GetSystemManufacturer(); }
        }

        /// <summary>
        /// Gets model.
        /// </summary>
        public string Model
        {
            get { return deviceService.GetDeviceModel(); }
        }

    }
#pragma warning restore 1998
}