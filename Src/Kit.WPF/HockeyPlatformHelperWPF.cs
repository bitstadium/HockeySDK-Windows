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
    public class HockeyPlatformHelperWPF : IHockeyPlatformHelper
    {

        private const string FILE_PREFIX = "HA__SETTING_";
        IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);

        private string PostfixWithUniqueAppString(string folderName, bool noDirectorySeparator = false)
        {
            return ((folderName ?? "") + (noDirectorySeparator ? "" : "" + Path.DirectorySeparatorChar) + HockeyClientWPFExtensions.AppUniqueFolderName);
        }

        /// <summary>
        /// Sets the setting value.
        /// </summary>
        /// <param name="key">Key value.</param>
        /// <param name="value">Value value.</param>
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "ToDo: Fix it later.")]
        public void SetSettingValue(string key, string value)
        {
            using (var fileStream = isoStore.OpenFile(PostfixWithUniqueAppString(FILE_PREFIX + key, true), FileMode.Create, FileAccess.Write))
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
            if(isoStore.FileExists(FILE_PREFIX + key)) {
                using (var fileStream = isoStore.OpenFile(PostfixWithUniqueAppString(FILE_PREFIX + key, true), FileMode.Open, FileAccess.Read))
                {
                    using(var reader = new StreamReader(fileStream)){
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
            if (isoStore.FileExists(PostfixWithUniqueAppString(FILE_PREFIX + key, true)))
            {
                isoStore.DeleteFile(PostfixWithUniqueAppString(FILE_PREFIX + key, true));
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
            if (isoStore.FileExists(PostfixWithUniqueAppString(folderName) + Path.DirectorySeparatorChar + fileName))
            {
                isoStore.DeleteFile(PostfixWithUniqueAppString(folderName) + Path.DirectorySeparatorChar + fileName);
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
            return isoStore.FileExists(PostfixWithUniqueAppString(folderName) + Path.DirectorySeparatorChar + fileName);
        }

        /// <summary>
        /// Gets stream.
        /// </summary>
        /// <param name="fileName">File name.</param>
        /// <param name="folderName">Folder name.</param>
        /// <returns>Stream object.</returns>
        public async Task<Stream> GetStreamAsync(string fileName, string folderName = null)
        {
            return isoStore.OpenFile(PostfixWithUniqueAppString(folderName) + Path.DirectorySeparatorChar + fileName, FileMode.Open, FileAccess.Read);
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
            if (!isoStore.DirectoryExists(PostfixWithUniqueAppString(folderName))) {
                isoStore.CreateDirectory(PostfixWithUniqueAppString(folderName));
            }

            using (var fileStream = isoStore.OpenFile(PostfixWithUniqueAppString(folderName) + Path.DirectorySeparatorChar + fileName,FileMode.Create,FileAccess.Write)) {
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
            try {
                return isoStore.GetFileNames(PostfixWithUniqueAppString(folderName) + Path.DirectorySeparatorChar + fileNamePattern ?? "*");
            } catch (DirectoryNotFoundException) {
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
            if (!isoStore.DirectoryExists(PostfixWithUniqueAppString(folderName)))
            {
                isoStore.CreateDirectory(PostfixWithUniqueAppString(folderName));
            }

            using (var fileStream = isoStore.OpenFile(PostfixWithUniqueAppString(folderName) + Path.DirectorySeparatorChar + fileName, FileMode.Create, FileAccess.Write))
            {
                dataStream.CopyTo(fileStream);
            }
        }

        #endregion

        string _appPackageName = null;

        /// <summary>
        /// Gets or sets application package name.
        /// </summary>
        public string AppPackageName
        {
            get
            {
                if(_appPackageName == null) {
                    _appPackageName = Application.Current.GetType().Namespace;
                }
                return _appPackageName;
            }
            set
            {
                _appPackageName = value;
            }
        }

        string _appVersion = null;

        /// <summary>
        /// Gets or sets application version.
        /// </summary>
        public string AppVersion
        {
            get { 
                if(_appVersion == null) {
                //ClickOnce
                    try
                    {
                        var type = Type.GetType("System.Deployment.Application.ApplicationDeployment");
                        object deployment = type.GetMethod("CurrentDeployment").Invoke(null, null);
                        Version version = type.GetMethod("CurrentVersion").Invoke(deployment, null) as Version;
                        _appVersion = version.ToString();
                    }
                    catch (Exception)
                    {
                        //Entry Assembly
                        _appVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();
                    }
                }
                return _appVersion ?? "0.0.0.1";
            }
            set {
                _appVersion = value;
            }
        }


        /// <summary>
        /// Gets OS version.
        /// </summary>
        public string OSVersion
        {
            get
            {
                //as windows 8.1 lies to us to be 8 we try via registry
                try
                {
                    using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion"))
                    {
                        return (string)registryKey.GetValue("CurrentVersion") + "." + (string)registryKey.GetValue("CurrentBuild") + ".0";
                    }
                }
                catch (Exception e)
                {
                    HockeyClient.Current.AsInternal().HandleInternalUnhandledException(e);
                }
                return Environment.OSVersion.Version.ToString() + " " + Environment.OSVersion.ServicePack; 
            }
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
            get
            {
                return Extensibility.SdkVersionPropertyContextInitializer.GetAssemblyVersion();
            }
        }


        /// <summary>
        /// Gets SDK name.
        /// </summary>
        public string SDKName
        {
            get
            { return HockeyConstants.SDKNAME; }
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
            get { 
                //TODO System.Management referenzieren !?
                /*
                Type.GetType
                ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
            //collection to store all management objects
            ManagementObjectCollection moc = mc.GetInstances();
            if (moc.Count != 0)
            {
                foreach (ManagementObject mo in mc.GetInstances())
                {
                 mo["Manufacturer"].ToString()
                */
                return null;
            }
        }

        /// <summary>
        /// Gets model.
        /// </summary>
        public string Model
        {
            get
            {
                //TODO siehe Manufacturer mit "Model"
                return null;
            }
        }

    }
#pragma warning restore 1998
}
