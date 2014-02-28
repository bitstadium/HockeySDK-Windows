using HockeyApp.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Browser;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using HockeyApp.Tools;
using Microsoft.Phone.Reactive;
using Windows.Phone.Management.Deployment;
using System.Windows;
using Windows.Management.Deployment;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Media;

namespace HockeyApp
{

    public enum UpdateCheckFrequency
    {
        Always
        //TODO daily/weekly/monthly
    }

    public enum UpdateMode
    {
        Startup,
        InApp
    }

    /// <summary>
    /// SEttings for update-checking
    /// </summary>
    public class UpdateCheckSettings
    {

        public static UpdateCheckSettings DefaultStartupSettings
        {
            get
            {
                return new UpdateCheckSettings();
           }
        }

        private UpdateMode updateMode = UpdateMode.Startup;
        /// <summary>
        /// Defines the mode in which the Startup-check should be run (InApp vs. during Startup)
        /// </summary>
        public UpdateMode UpdateMode
        {
            get { return updateMode; }
            set { updateMode = value; }
        }
        
        private UpdateCheckFrequency updateCheckFrequency = UpdateCheckFrequency.Always;
        /// <summary>
        /// Set the frequency to check for updates
        /// </summary>
        public UpdateCheckFrequency UpdateCheckFrequency
        {
            get { return updateCheckFrequency; }
            set { updateCheckFrequency = value; }
        }
        
        private Func<IAppVersion,bool> customDoShowUpdateFunc = null;
        /// <summary>
        /// Handle a found update with custom code (no default ui shown)
        /// </summary>
        public Func<IAppVersion,bool> CustomDoShowUpdateFunc
        {
            get { return customDoShowUpdateFunc; }
            set { customDoShowUpdateFunc = value; }
        }

        private bool enforceUpdateIfMandatory = true;
        /// <summary>
        /// Enforce the update if new version is marked as mandatory (default: true)
        /// </summary>
        public bool EnforceUpdateIfMandatory
        {
            get { return enforceUpdateIfMandatory; }
            set { enforceUpdateIfMandatory = value; }
        }

    }

    public class UpdateManager
    {

        private static readonly UpdateManager instance = new UpdateManager();

        static UpdateManager() { }
        private UpdateManager() { }

        public static UpdateManager Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// Check for an update on the server
        /// HockecCient needs to be configured before calling this method (normally done by configuring a crahshandler in the App() constructor)
        /// </summary>
        /// <param name="settings">[optional] custom settings</param>
        public void RunUpdateCheck(UpdateCheckSettings settings = null)
        {
            UpdateVersionIfAvailable(settings ?? UpdateCheckSettings.DefaultStartupSettings);
        }

        /// <summary>
        /// Obsolete. Use UpdateManager.Instance.RunUpdateCheck()
        [Obsolete]
        public static void RunUpdateCheck(string identifier, UpdateCheckSettings settings = null)
        {
            Instance.UpdateVersionIfAvailable(settings ?? UpdateCheckSettings.DefaultStartupSettings);
        }

        internal void UpdateVersionIfAvailable(UpdateCheckSettings updateCheckSettings)
        {
            if (CheckWithUpdateFrequency(updateCheckSettings.UpdateCheckFrequency) && NetworkInterface.GetIsNetworkAvailable())
            {
                var task = HockeyClient.Instance.GetAppVersionsAsync();
                task.ContinueWith((finishedTask) =>
                {
                    var appVersions = finishedTask.Result;
                    var newestAvailableAppVersion = appVersions.FirstOrDefault();

                    var currentVersion = new Version(ManifestHelper.GetAppVersion());
                    if (appVersions.Any()
                        && new Version(newestAvailableAppVersion.Version) > currentVersion
                        && (updateCheckSettings.CustomDoShowUpdateFunc == null || updateCheckSettings.CustomDoShowUpdateFunc(newestAvailableAppVersion)))
                    {
                        if (updateCheckSettings.UpdateMode.Equals(UpdateMode.InApp) || (updateCheckSettings.EnforceUpdateIfMandatory && newestAvailableAppVersion.Mandatory))
                        {
                            ShowVersionPopup(currentVersion, appVersions, updateCheckSettings);
                        }
                        else
                        {
                            ShowUpdateNotification(currentVersion, appVersions, updateCheckSettings);
                        }
                    }
                    else
                    {
                        if (updateCheckSettings.UpdateMode.Equals(UpdateMode.InApp))
                        {
                            Scheduler.Dispatcher.Schedule(() => MessageBox.Show(LocalizedStrings.LocalizedResources.NoUpdateAvailable));
                        }
                    }
                });
            }
        }

        internal bool CheckWithUpdateFrequency(UpdateCheckFrequency frequency)
        {
            //TODO implement. store and check last update timestamp...
            return true;
        }

        protected void ShowUpdateNotification(Version currentVersion, IEnumerable<IAppVersion> appVersions, UpdateCheckSettings updateCheckSettings)
        {
            Scheduler.Dispatcher.Schedule(() =>
            {
                NotificationTool.Show(
                    LocalizedStrings.LocalizedResources.UpdateNotification,
                    LocalizedStrings.LocalizedResources.UpdateAvailable,
                    new NotificationAction(LocalizedStrings.LocalizedResources.Show, (Action) (() =>
                    {
                        ShowVersionPopup(currentVersion, appVersions, updateCheckSettings);
                    })),
                    new NotificationAction(LocalizedStrings.LocalizedResources.Dismiss, (Action) (() =>
                    {
                        //DO nothing
                    }))
                );
            });
        }

        protected void ShowVersionPopup(Version currentVersion, IEnumerable<IAppVersion> appVersions, UpdateCheckSettings updateCheckSettings)
        {
            Scheduler.Dispatcher.Schedule(() =>
            {
                //TODO hooks for customizing
                UpdatePopupTool.ShowPopup(currentVersion, appVersions, updateCheckSettings, DoUpdate);
            });
        }

        internal async void DoUpdate(IAppVersion availableUpdate)
        {
            var aetxUri = new Uri(Constants.ApiBase + "apps/" + HockeyClient.Instance.AppIdentifier + ".aetx", UriKind.Absolute);
            var downloadUri = new Uri(Constants.ApiBase + "apps/" + HockeyClient.Instance.AppIdentifier + "/app_versions/" + availableUpdate.Id + ".xap", UriKind.Absolute);

            //it won't get the result anyway because this app-instance will get killed during the update
            //TODO try catch to show message to the user?
            var result = await InstallationManager.AddPackageAsync(availableUpdate.Title, downloadUri);

        }
    }
}
