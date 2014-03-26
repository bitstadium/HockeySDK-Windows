using HockeyApp.Gui;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace HockeyApp
{
    internal class UpdateManager:IUpdateManager
    {
        private bool _autoShowUi = true;
        private Version _localVersion;

        private ILog _logger;
        internal UpdateManager()
        {
            this._logger = HockeyLogManager.GetLog(this.GetType());
        }

        private Func<bool> _shutdownActions;

        public void CheckForUpdates(bool autoShowUi, Func<bool> shutdownActions = null, Action<IAppVersion> updateAvailableAction = null)
        {
            Task t = CheckForUpdatesAsync(autoShowUi, shutdownActions, updateAvailableAction);
            t.Wait();
        }

        public async Task CheckForUpdatesAsync(bool autoShowUi, Func<bool> shutdownActions = null, Action<IAppVersion> updateAvailableAction = null)
        {
            this._autoShowUi = autoShowUi;
            this._shutdownActions = shutdownActions;

            if (autoShowUi && shutdownActions == null)
            {
                throw new ArgumentException("You have to provide a shutdownRequest-Action if using autoUi == true!");
            }

            try
            {
                IAppVersion newestVersion = await CheckForUpdates();
                if (newestVersion != null)
                {
                    if (updateAvailableAction != null) { updateAvailableAction(newestVersion); }
                    if (this._autoShowUi)
                    {
                        this.StartUi(newestVersion);
                    }
                }
                
            }
            catch (Exception ex)
            {
                this._logger.Warn("Exception in CheckForUpdatesAsync: " + ex.GetType().Name + "\n" + ex.Message);
            }
        }

        private async Task<IAppVersion> CheckForUpdates()
        {
            if(System.Version.TryParse(HockeyClient.Instance.VersionInfo, out this._localVersion)){
                var appVersions = await HockeyClient.Instance.GetAppVersionsAsync();
                IAppVersion newest = appVersions.FirstOrDefault();
                if (newest != null)
                {
                    this._logger.Info("Found remote version. Version = " + newest.Version);
                    Version remoteVersion = null;
                    if (Version.TryParse(newest.Version, out remoteVersion))
                    {
                        if (remoteVersion > this._localVersion)
                        {
                            this._logger.Info("Remote version is newer than local version.");
                            return newest;
                        }
                        else
                        {
                            this._logger.Info("Local version is up to date.");
                        }
                    }
                    else
                    {
                        this._logger.Warn("Remote version cannot be formatted to System.Version. CheckForUpdates canceled. Remote version: " + newest.Version);
                    }
                }
                else
                {
                    this._logger.Warn("No remote version found!");
                }
            }else{
                this._logger.Warn("Local version cannot be formatted to System.Version. CheckForUpdates canceled. Local version: " + HockeyClient.Instance.VersionInfo);
            }
            return null;
        }


        private void StartUi(IAppVersion newVersion)
        {
            UpdateWindow window = new UpdateWindow(newVersion, this._localVersion);
            window.Show();
        }
        
    }
}
