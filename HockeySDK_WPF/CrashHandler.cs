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


        internal CrashHandler(IHockeyClient hockeyClient, Func<Exception, string> descriptionLoader, bool keepRunning)
        {
            this._hockeyClient = hockeyClient;
            this._descriptionLoader = descriptionLoader;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            if (keepRunning)
            {
                Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            }
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
            e.Handled = true;
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
                    Version = HockeyClient.Current.AsInternal().VersionInfo,
                    OperatingSystem = Environment.OSVersion.Platform.ToString(),
                    Windows = Environment.OSVersion.Version.ToString() + Environment.OSVersion.ServicePack,
                    Manufacturer = "",
                    Model = ""
                };

                ICrashData crash = HockeyClient.Current.AsInternal().CreateCrashData(e, logInfo);
                using (FileStream stream = File.Create(Path.Combine(Constants.GetPathToHockeyCrashes(), filename)))
                {
                    crash.Serialize(stream);
                    stream.Flush();
                }
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

            //System Semaphore would be another possibility. But the worst thing that can happen now, is
            //that a crash is send twice.
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
                            using (FileStream fs = File.Open(crashFileName, FileMode.Open, FileAccess.ReadWrite))
                            {
                                ICrashData cd = HockeyClient.Current.AsInternal().Deserialize(fs);
                                await cd.SendDataAsync();
                            }
                            //if the process switch occurs between those lines the worst that can happen is that a crash is sent twice.
                            File.Delete(crashFileName);
                            logger.Info("Crashfile sent and deleted: {0}", crashFileName);
                        }
                        catch (Exception ex)
                        {
                            this.logger.Error(ex);
                        }
                    }
                }
            }
            System.Threading.Monitor.Exit(this);
        }
        #endregion
    }
}
