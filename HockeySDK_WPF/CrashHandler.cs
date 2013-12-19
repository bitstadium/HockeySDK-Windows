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

namespace HockeyApp
{
    internal class CrashHandler
    {
        #region ctor
        private ILog logger = HockeyLogManager.GetLog(typeof(CrashHandler));
        private IHockeyClient _hockeyClient = null;
        private  Func<Exception, string> _descriptionLoader = null;

        internal CrashHandler(IHockeyClient hockeyClient, Func<Exception, string> descriptionLoader)
        {
            this._hockeyClient = hockeyClient;
            this._descriptionLoader = descriptionLoader;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        #endregion

        #region Exception Handler
        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            logger.Info("Caught unobserved exception from TaskScheduler! Type={0}, Message={1}", new object[] { e.Exception.GetType().Name, e.Exception.Message });
            HandleException(e.Exception);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;

            if (ex != null)
            {
                logger.Info ("Caught unobserved exception from AppDomain! Type={0}, Message={1}", new object [] { ex.GetType ().Name, ex.Message });
                HandleException(ex);
            }
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            logger.Info ("Caught unobserved exception from Dispatcher! Type={0}, Message={1}", new object [] { e.Exception.GetType ().Name, e.Exception.Message });
            HandleException(e.Exception);
        }

        private void HandleException(Exception e)
        {
            try
            {
                string crashID = Guid.NewGuid().ToString();
                String filename = String.Format("{0}{1}.log", Constants.CrashFilePrefix, crashID);

                CrashLogInformation logInfo = new CrashLogInformation()
                {
                    PackageName = Application.Current.GetType().Namespace,
                    Version = HockeyClient.Instance.VersionInfo,
                    OperatingSystem = Environment.OSVersion.ToString(),
                    Manufacturer = "",
                    Model = ""
                };

                ICrashData crash = HockeyClient.Instance.CreateCrashData(e,logInfo);
                FileStream stream = File.Create(Path.Combine(Constants.GetPathToHockeyCrashes(), filename));
                crash.Serialize(stream);
                stream.Flush();
                stream.Close();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }
        #endregion

        #region Send Crashes

        internal string[] GetCrashFiles()
        {
            return Directory.GetFiles(Constants.GetPathToHockeyCrashes(), Constants.CrashFilePrefix + "*.log");
        }

        internal void DeleteAllCrashes()
        {
            foreach (string filename in GetCrashFiles())
            {
                try
                {
                    File.Delete(filename);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            }
        }

        internal bool CrashesAvailable { get { return GetCrashFiles().Length > 0; } }
        internal int CrashesAvailableCount { get { return GetCrashFiles().Length; } }

        internal async Task SendCrashesNowAsync()
        {
            if (!System.Threading.Monitor.TryEnter(this))
            {
                logger.Warn("Sending crashes was called multiple times!");
                throw new Exception("Hockey is already sending crashes to server!");
            }
            else
            {
                logger.Info("Start send crashes to platform.");

                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    foreach (string crashFileName in this.GetCrashFiles())
                    {
                        logger.Info("Crashfile found: {0}", crashFileName);
                        try
                        {
                            FileStream fs = File.OpenRead(crashFileName);
                            ICrashData cd = HockeyClient.Instance.Deserialize(fs);
                            fs.Close();
                            await cd.SendDataAsync();
                            File.Delete(crashFileName);
                            logger.Info("Crashfile sent and deleted: {0}", crashFileName);
                        }
                        catch (WebTransferException ex)
                        {
                            this.logger.Error(ex);
                        }
                        catch (Exception ex)
                        {
                            this.logger.Error(ex);
                            File.Delete(crashFileName);
                        }
                    }
                }
            }
            System.Threading.Monitor.Exit(this);
        }
        #endregion
    }
}
