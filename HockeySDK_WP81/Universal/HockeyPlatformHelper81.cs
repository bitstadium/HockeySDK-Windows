using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using HockeyApp.Tools;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.Security.ExchangeActiveSyncProvisioning;

namespace HockeyApp
{
    internal partial class HockeyPlatformHelper81 : IHockeyPlatformHelper
    {
        public HockeyPlatformHelper81()
        {
            AppxManifest.InitializeManifest();
            clientDeviceInfo = new EasClientDeviceInformation();
            Task x = SetOSVersionAsync();
        }

        private EasClientDeviceInformation clientDeviceInfo;

        public void SetSettingValue(string key, string value)
        {
            ApplicationData.Current.LocalSettings.Values.SetValue(key, value);
        }

        public string GetSettingValue(string key)
        {
            return ApplicationData.Current.LocalSettings.Values.GetValue(key);
        }

        public void RemoveSettingValue(string key)
        {
            ApplicationData.Current.LocalSettings.Values.RemoveValue(key);
        }

        public async Task WriteStreamToFileAsync(System.IO.Stream dataStream, string fileName, string folderName = null)
        {
            var folder = await folderName.GetLocalFolderByNameCreateIfNotExistingAsync();
            var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            using (var stream = await file.OpenStreamForWriteAsync())
            {
                await dataStream.CopyToAsync(stream);
            }
        }

        public async Task<IEnumerable<string>> GetFileNamesAsync(string folderName = null, string fileNamePattern = null)
        {
            var folder = await folderName.GetLocalFolderByNameCreateIfNotExistingAsync();
            var allfilenames = (await folder.GetFilesAsync()).Select(f => f.Name);
            if (fileNamePattern != null)
            {
                var regex = fileNamePattern.RegexForLikeMatching(fileNamePattern);
                return allfilenames.Where(f => regex.IsMatch(f));
            }
            else
            {
                return allfilenames;
            }
        }

        public async Task<bool> DeleteFileAsync(string fileName, string folderName = null)
        {
            var folder = await folderName.GetLocalFolderByNameCreateIfNotExistingAsync();
            StorageFile file = null;
            try {
                file = await folder.GetFileAsync(fileName);
            } catch(Exception) {}
            if(file!= null) {
                await file.DeleteAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> FileExistsAsync(string fileName, string folderName = null)
        {
            var folder = await folderName.GetLocalFolderByNameCreateIfNotExistingAsync();
            StorageFile file = null;
            try {
                file = await folder.GetFileAsync(fileName);
            } catch(Exception) {}
            return file != null;
        }

        public async Task<System.IO.Stream> GetStreamAsync(string fileName, string folderName = null)
        {
            var folder = await folderName.GetLocalFolderByNameCreateIfNotExistingAsync();
            StorageFile file = null;
            try
            {
                file = await folder.GetFileAsync(fileName);
            }
            catch (Exception) { }
            if (file != null)
            {
                return await file.OpenStreamForReadAsync();
            }
            return null;
        }

        public string AppVersion
        {
            get { return AppxManifest.Current.Package.Identity.Version; }
        }

        public string AppPackageName
        {
            get { return Application.Current.GetType().Namespace; }
        }

        public string Manufacturer
        {
            get { return clientDeviceInfo.SystemManufacturer; }
        }

        public string Model
        {
            get { return clientDeviceInfo.SystemProductName; } 
        }


        public string OSVersion { get; set; }

        
        // Workaround to get Windows Version http://stackoverflow.com/a/18893609
        private async Task SetOSVersionAsync()
        {
            var t = new TaskCompletionSource<string>();
            var w = new WebView();
            w.NavigateToString("<html />");
            NotifyEventHandler h = null;
            h = (s, e) =>
            {
                try
                {
                    var match = Regex.Match(e.Value, @"Windows\s+NT\s+\d+(\.\d+)?");
                    if (match.Success)
                        t.SetResult(match.Value);
                    else
                        t.SetResult("Unknown");
                }
                catch (Exception ex) { t.SetException(ex); }
                finally { /* release */ w.ScriptNotify -= h; }
            };
            w.ScriptNotify += h;
            await w.InvokeScriptAsync("execScript", new[] { "window.external.notify(navigator.appVersion); " });
            this.OSVersion = t.Task.Result;
        }


        
    }
}
