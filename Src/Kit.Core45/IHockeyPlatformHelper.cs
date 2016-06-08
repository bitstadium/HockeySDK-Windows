using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.HockeyApp
{
    /// <summary>
    /// interface for 
    /// </summary>
    public interface IHockeyPlatformHelper
    {
        #region App-Settings        
        /// <summary>
        /// Saves the given value under the specified key in app settings
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        void SetSettingValue(String key, String value);

        /// <summary>
        /// gets the value for the given key from app settings
        /// </summary>
        /// <param name="key"></param>
        /// <returns>the vlaue or null if the setting with the given key does not exist.</returns>
        string GetSettingValue(String key);
        /// <summary>
        /// Removes the value for the given key from settings.
        /// </summary>
        /// <param name="key">The key.</param>
        void RemoveSettingValue(String key);

        #endregion

        #region File-Access        
        /// <summary>
        /// Writes the stream asynchronous to a file with the given filename and folder.
        /// </summary>
        /// <param name="dataStream">The data stream.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="folderName">Name of the folder.</param>
        /// <returns></returns>
        Task WriteStreamToFileAsync(Stream dataStream, string fileName, string folderName = null);
        /// <summary>
        /// Indicates whether the implementing platform supports synchronized write.
        /// </summary>
        bool PlatformSupportsSyncWrite { get; }
        /// <summary>
        /// Writes the stream synchronous to a file with the given filename and folder.
        /// </summary>
        /// <param name="dataStream">The data stream.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="folderName">Name of the folder.</param>
        void WriteStreamToFileSync(Stream dataStream, string fileName, string folderName = null);
        /// <summary>
        /// Gets the file names of files in a directory asynchronous.
        /// </summary>
        /// <param name="folderName">Name of the folder.</param>
        /// <param name="fileNamePattern">The file name pattern.</param>
        /// <returns></returns>
        Task<IEnumerable<String>> GetFileNamesAsync(string folderName = null, string fileNamePattern = null);
        /// <summary>
        /// Gets the stream of a file asynchronous.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="folderName">Name of the folder.</param>
        /// <returns></returns>
        Task<Stream> GetStreamAsync(string fileName, string folderName = null);
        /// <summary>
        /// Deletes the file asynchronous.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="folderName">Name of the folder.</param>
        /// <returns></returns>
        Task<bool> DeleteFileAsync(string fileName, string folderName = null);
        /// <summary>
        /// Returns if the file with the given name and folder exists asynchronous.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="folderName">Name of the folder.</param>
        /// <returns></returns>
        Task<bool> FileExistsAsync(string fileName, string folderName = null);
        #endregion

        #region PlatformInfos
        /// <summary>
        /// The version of your app - determined by platform-specific best practice
        /// </summary>
        string AppVersion { get;}
        /// <summary>
        ///  PackageName of your app - the namespace oft the App class
        /// </summary>
        string AppPackageName { get;}
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
        /// Version of the HockeyApp SDK (which implements this PlatformHelper)
        /// </summary>
        string SDKVersion { get; }
        /// <summary>
        /// Name of the HockeyApp SDK (which implements this PlatformHelper)
        /// </summary>
        string SDKName { get; }
        /// <summary>
        /// UserAgent Header to be sent to HockeyApp
        /// </summary>
        string UserAgentString { get; }
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
}
