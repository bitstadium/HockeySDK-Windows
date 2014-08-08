using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HockeyApp
{

    public interface IHockeyPlatformHelper
    {
        #region App-Settings
        void SetSettingValue(String key, String value);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns>the vlaue or null if the setting with the given key does not exist.</returns>
        string GetSettingValue(String key);
        void RemoveSettingValue(String key);

        #endregion

        #region File-Access
        Task WriteStreamToFileAsync(Stream dataStream, string fileName, string folderName = null);
        Task<IEnumerable<String>> GetFileNamesAsync(string folderName = null, string fileNamePattern = null);
        Task<Stream> GetStreamAsync(string fileName, string folderName = null);
        Task<bool> DeleteFileAsync(string fileName, string folderName = null);
        Task<bool> FileExistsAsync(string fileName, string folderName = null);
        #endregion

        #region PlatformInfos
        /// <summary>
        /// The version of your app - determined by platform-specific best practice
        /// </summary>
        string AppVersion { get;}
        /// <summary>
        /// Operating system version
        /// </summary>
        string OSVersion { get; }
        /// <summary>
        /// Operating system platform
        /// </summary>
        string OSPlatform { get; }
        /// <summary>
        /// Product Id from Manifest
        /// </summary>
        string ProductID { get; }
        /// <summary>
        /// Manufacturer of the device
        /// </summary>
        string Manufacturer { get; }
        /// <summary>
        /// Device model
        /// </summary>
        string Model { get; }
        #endregion
    }

    internal static class PlatformHelperExtensions
    {
        internal static string GetWindowsVersionString(this IHockeyPlatformHelper @this)
        {
            return @this.OSPlatform.Contains("Phone") ? String.Empty : @this.OSVersion;
        }

        internal static string GetWindowsPhoneVersionString(this IHockeyPlatformHelper @this)
        {
            return @this.OSPlatform.Contains("Phone") ? @this.OSVersion : String.Empty;
        }

    }
}
