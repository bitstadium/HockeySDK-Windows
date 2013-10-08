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
            logger.Info("Catched unobserved exception from TaskScheduler! Type={0}, Message={1}", new object[] { e.Exception.GetType().Name, e.Exception.Message });
            HandleException(e.Exception);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;

            if (ex != null)
            {
                logger.Info("Catched unobserved exception from AppDomain! Type={0}, Message={1}", new object[] { ex.GetType().Name, ex.Message });
                HandleException(ex);
            }
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            logger.Info("Catched unobserved exception from Dispatcher! Type={0}, Message={1}", new object[] { e.Exception.GetType().Name, e.Exception.Message });
            HandleException(e.Exception);
            e.Handled = true;
        }

        private void HandleException(Exception e)
        {
            new Crash(e, this._descriptionLoader).Save();
        }
        #endregion

        #region Send Crashes
        internal async Task SendCrashesNow()
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
                    IEnumerable<Crash> availableCrashes = Crash.GetCrashes();
                    if (availableCrashes.Count() > 0)
                    {
                        logger.Info("Crashes available: {0}", new object[] { availableCrashes.Count().ToString() });

                        foreach (Crash crash in availableCrashes)
                        {
                            try
                            {
                                await this._hockeyClient.PostCrashAsync(crash.Log, crash.UserID, crash.ContactInformation, crash.Description);
                                crash.Delete();
                            }
                            catch (Exception ex)
                            {
                                this.logger.Error(ex);
                            }
                        }

                    }
                    else
                    {
                        logger.Info("No crashes available.");
                    }
                }
            }
            System.Threading.Monitor.Exit(this);
        }
        #endregion
    }
}
