using HockeyApp.Exceptions;
using HockeyApp.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using Windows.Storage;
using Windows.UI.Xaml;

namespace HockeyApp
{
    internal class CrashHandler
    {
        #region ctor
        private ILog logger = HockeyLogManager.GetLog(typeof(CrashHandler));
        private IHockeyClient _hockeyClient = null;
        private Func<Exception, string> _descriptionLoader = null;

        private CrashLogInformation _logInfo = new CrashLogInformation();

        private void InitCrashLogInformation()
        {
            XElement xml = XElement.Load("AppxManifest.xml");
            string ns = xml.GetDefaultNamespace().NamespaceName;

            XElement identityElement = xml.Element(XName.Get("Identity", ns));

            this._logInfo.PackageName = Application.Current.GetType().Namespace;
            this._logInfo.Version = identityElement.Attribute("Version").Value.ToString();
            this._logInfo.OperatingSystem = "WinRT";
            this._logInfo.ProductID = identityElement.Attribute("Name").Value.ToString();
        }


        internal CrashHandler(IHockeyClient hockeyClient, Func<Exception, string> descriptionLoader)
        {
            this._hockeyClient = hockeyClient;
            this._descriptionLoader = descriptionLoader;
            InitCrashLogInformation();
            Application.Current.UnhandledException += Current_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        #endregion

        #region Exception Handler
        private async void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            logger.Info("Catched unobserved exception from TaskScheduler! Type={0}, Message={1}", new object[] { e.Exception.GetType().Name, e.Exception.Message });
            await HandleException(e.Exception);
        }

        private async void Current_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            logger.Info("Catched unobserved exception from Dispatcher! Type={0}, Message={1}", new object[] { e.Exception.GetType().Name, e.Exception.Message });
            await HandleException(e.Exception);
            
            Application.Current.Exit();
        }

        private async Task HandleException(Exception e)
        {
            var crashData = HockeyClient.Instance.CreateCrashData(e, this._logInfo);
            var crashId = Guid.NewGuid();

            try
            {
                var store = ApplicationData.Current.LocalFolder;
                var folder = await store.CreateFolderAsync(Constants.CRASHPATHNAME, CreationCollisionOption.OpenIfExists);

                var filename = string.Format("{0}{1}.log", Constants.CrashFilePrefix, crashId);

                

                var file = await folder.CreateFileAsync(filename);
                using (var stream = await file.OpenStreamForWriteAsync())
                {
                    crashData.Serialize(stream);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }
        #endregion

        #region Send Crashes

        internal async Task<List<StorageFile>> GetCrashFiles()
        {
            List<StorageFile> retVal = new List<StorageFile>();
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFolder crashFolder = null;
            try
            {
                crashFolder = await localFolder.GetFolderAsync(Constants.CRASHPATHNAME);
            }
            catch { }
            if (crashFolder != null)
            {
                foreach (StorageFile current in await crashFolder.GetFilesAsync())
                {
                    if (current.Name.StartsWith(Constants.CrashFilePrefix) && current.Name.EndsWith(".log"))
                    {
                        retVal.Add(current);
                    }
                }
            }
            return retVal;
        }

        internal async Task DeleteAllCrashesAsync()
        {
            foreach (StorageFile file in await GetCrashFiles())
            {
                try
                {
                    await file.DeleteAsync();
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            }
        }

        internal bool CrashesAvailable
        {
            get
            {
                return this.CrashesAvailableCount > 0;
            }
        }
        internal int CrashesAvailableCount
        {
            get
            {
                Task<List<StorageFile>> t = GetCrashFiles();
                t.Wait();
                return t.Result.Count;
            }
        }

        internal async Task SendCrashesNowAsync()
        {
            bool deleteFlag = false; //necessary, because no await allowed in catch body

            logger.Info("Start send crashes to platform.");

            if (NetworkInterface.GetIsNetworkAvailable())
            {
                foreach (StorageFile crashFile in await this.GetCrashFiles())
                {
                    logger.Info("Crashfile found: {0}", crashFile.Name);
                    deleteFlag = false;
                    try
                    {
                        Stream fs = await crashFile.OpenStreamForReadAsync();
                        ICrashData cd = HockeyClient.Instance.Deserialize(fs);
                        fs.Dispose();
                        await cd.SendDataAsync();
                        await crashFile.DeleteAsync();
                        logger.Info("Crashfile deleted: {0}", crashFile.Name);
                    }
                    catch (WebTransferException ex)
                    {
                        this.logger.Error(ex);
                    }
                    catch (Exception ex)
                    {
                        this.logger.Error(ex);
                        deleteFlag = true;
                    }
                    if (deleteFlag)
                    {
                        await crashFile.DeleteAsync();
                    }
                }
            }
        }
        #endregion
    }
}