using Microsoft.Phone.Info;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;
using HockeyApp.Tools;
using System.Windows;

namespace HockeyApp
{
    internal class HockeyPlatformHelperWP8SL : IHockeyPlatformHelper
    {
        IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;

        #region settings
        public void SetSettingValue(string key, string value)
        {
            settings.SetValue(key, value);
        }

        public string GetSettingValue(string key)
        {
            return settings.GetValue(key).ToString();
        }

        public void RemoveSettingValue(string key)
        {
            settings.RemoveValue(key);
        }


        #endregion

        #region File access

        public async Task WriteStreamToFileAsync(Stream dataStream, string fileName, string folderName = null)
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            if (folderName != null)
            {
                localFolder = await localFolder.CreateFolderAsync(folderName,CreationCollisionOption.OpenIfExists);
            }
            using(var stream = await localFolder.OpenStreamForWriteAsync(fileName, CreationCollisionOption.ReplaceExisting)){
                await dataStream.CopyToAsync(stream);
            }
        }

        public async Task<IEnumerable<string>> GetFileNamesAsync(string folderName = null, string fileNamePattern = null)
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            if (folderName != null)
            {
                try
                {
                    localFolder = await localFolder.GetFolderAsync(folderName);
                }
                catch (Exception)
                {
                    return new List<string>();
                }
            }
            var allfilenames = (await localFolder.GetFilesAsync()).Select(f => f.Name); 
            if(fileNamePattern != null) {
                var regex = fileNamePattern.RegexForLikeMatching(fileNamePattern);
                return allfilenames.Where(f => regex.IsMatch(f));
            } else {
                return allfilenames;
            }
        }

        public async Task<bool> DeleteFileAsync(string fileName, string folderName = null)
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            if (folderName != null)
            {
                try
                {
                    localFolder = await localFolder.GetFolderAsync(folderName);
                }
                catch (Exception)
                {
                    return false;
                }
            }
            try
            {
                await (await localFolder.GetFileAsync(fileName)).DeleteAsync();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public async Task<bool> FileExistsAsync(string fileName, string folderName = null)
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            if (folderName != null)
            {
                try
                {
                    localFolder = await localFolder.GetFolderAsync(folderName);
                }
                catch (Exception)
                {
                    return false;
                }
            }
            try
            {
                await localFolder.GetFileAsync(fileName);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<Stream> GetStreamAsync(string fileName, string folderName = null)
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            if (folderName != null)
            {
                try
                {
                    localFolder = await localFolder.GetFolderAsync(folderName);
                }
                catch (Exception)
                {
                    return null;
                }
            }
            try
            {
                return await localFolder.OpenStreamForReadAsync(fileName);
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion

        #region infos
        public string AppVersion
        {
            get { return ManifestHelper.GetAppVersion(); }
        }

        string _appPackageName = null;
        public string AppPackageName
        {
            get
            {
                if (_appPackageName == null)
                {
                    _appPackageName = Application.Current.GetType().Namespace;
                }
                return _appPackageName;
            }
            set
            {
                _appPackageName = value;
            }
        }

        public string OSVersion
        {
            get { return Environment.OSVersion.Version.ToString(); }
        }

        public string OSPlatform
        {
            get { return "Windows Phone"; }
        }

        public string ProductID
        {
            get { return ManifestHelper.GetProductID(); }
        }

        public string Manufacturer
        {
            get
            {
                object manufacturer;
                if (DeviceExtendedProperties.TryGetValue("DeviceManufacturer", out manufacturer))
                {
                    return manufacturer.ToString();
                }
                else
                {
                    return null;
                }
            }
        }

        public string Model
        {
            get
            {
                object model;
                if (DeviceExtendedProperties.TryGetValue("DeviceName", out model))
                {
                    return model.ToString();
                }
                else
                {
                    return null;
                }
            }
        }
        #endregion



        
    }
}
