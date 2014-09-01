using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HockeyApp
{
    public class HockeyPlatformHelperWPF : IHockeyPlatformHelper
    {

        private const string FILE_PREFIX = "HA__SETTING_";
        IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);

        public void SetSettingValue(string key, string value)
        {
            using (var fileStream = isoStore.OpenFile(FILE_PREFIX + key,FileMode.Create, FileAccess.Write)){
                using (var writer = new StreamWriter(fileStream))
                {
                    writer.Write(value);
                }
            }
        }

        public string GetSettingValue(string key)
        {
            if(isoStore.FileExists(FILE_PREFIX + key)) {
                using (var fileStream = isoStore.OpenFile(FILE_PREFIX + key,FileMode.Open, FileAccess.Read)){
                    using(var reader = new StreamReader(fileStream)){
                        return reader.ReadToEnd();
                    }
                }
            }
            return null;
        }

        public void RemoveSettingValue(string key)
        {
            if(isoStore.FileExists(FILE_PREFIX + key)) {
                isoStore.DeleteFile(FILE_PREFIX + key);
            }
        }


        #region File access

        public async Task<bool> DeleteFileAsync(string fileName, string folderName = null)
        {
            if (isoStore.FileExists((folderName ?? "") + Path.DirectorySeparatorChar + fileName))
            {
                isoStore.DeleteFile((folderName ?? "") + Path.DirectorySeparatorChar + fileName);
                return true;
            }
            return false;
        }

        public async Task<bool> FileExistsAsync(string fileName, string folderName = null)
        {
            return isoStore.FileExists((folderName ?? "") + Path.DirectorySeparatorChar + fileName);
        }

        public async Task<Stream> GetStreamAsync(string fileName, string folderName = null)
        {
            return isoStore.OpenFile((folderName ?? "") + Path.DirectorySeparatorChar + fileName, FileMode.Open, FileAccess.Read);
        }

        public async Task WriteStreamToFileAsync(Stream dataStream, string fileName, string folderName = null)
        {
            // Ensure crashes folder exists
            if (!isoStore.DirectoryExists(folderName)) {
                isoStore.CreateDirectory(folderName);
            }

            using (var fileStream = isoStore.OpenFile((folderName ?? "") + Path.DirectorySeparatorChar + fileName,FileMode.Create,FileAccess.Write)) {
                await dataStream.CopyToAsync(fileStream);
            }
        }

        public async Task<IEnumerable<string>> GetFileNamesAsync(string folderName = null, string fileNamePattern = null)
        {
            try {
                return isoStore.GetFileNames((folderName ?? "") + Path.DirectorySeparatorChar + fileNamePattern ?? "*");
            } catch (DirectoryNotFoundException) {
                return new string[0];
            }
        }

        #endregion


        string _appPackageName = null;
        public string AppPackageName
        {
            get
            {
                if(_appPackageName == null) {
                    _appPackageName = Assembly.GetExecutingAssembly().EntryPoint.DeclaringType.Namespace;
                }
                return _appPackageName;
            }
            set
            {
                _appPackageName = value;
            }
        }

        string _appVersion = null;
        public string AppVersion
        {
            get { 

                if(_appVersion == null) {
                //ClickOnce
                    try {
                        var type = Type.GetType("System.Deployment.Application.ApplicationDeployment");
                        object deployment = type.GetMethod("CurrentDeployment").Invoke(null,null);
                        Version version = type.GetMethod("CurrentVersion").Invoke(deployment, null) as Version;
                        _appVersion = version.ToString();
                    } catch (Exception e) { }
                //Excecuting Assembly
                    _appVersion = Assembly.GetCallingAssembly().GetName().Version.ToString();
                }
                return _appVersion ?? "0.0.0-unknown";
            }
            set {
                _appVersion = value;
            }
        }

        public string OSVersion
        {
            get { return Environment.OSVersion.Version.ToString() + " " + Environment.OSVersion.ServicePack; }
        }

        public string OSPlatform
        {   //TODO wirklich ?!
            get { return (Type.GetType("Mono.Runtime") == null) ? "Windows" : "Mono"; }
        }

        public string SDKVersion
        {
            get { return Constants.SDKVERSION; }
        }

        public string SDKName
        {
            get
            { return Constants.SDKNAME; }
        }

        public string UserAgentString
        {
            get { return Constants.USER_AGENT_STRING; }
        }

        private string _productID = null;
        public string ProductID
        {
            get { return _productID; }
            set { _productID = value; }
        }
        
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

        public string Model
        {
            get
            {
                //TODO siehe Manufacturer mit "Model"
                return null;
            }
        }
        
    }
}
