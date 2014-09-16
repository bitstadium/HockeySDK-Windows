using HockeyApp.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using HockeyApp.Tools;
using Windows.Phone.Management.Deployment;
using System.Windows;
using Windows.Management.Deployment;
using Windows.UI.Popups;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using HockeyApp.Views;

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
    /// Settings for update-checking
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
        /// Defines the mode in which the Startup-check should be run (InApp vs. during Startup, with InApp a message is shown if no update is available)
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
        /// Handle a found update with custom code (return a boolean to indicate if default ui should be shown)
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

    /// <summary>
    /// Provides automatic update functionality with HockeyApp
    /// </summary>
    internal class UpdateManager
    {
        private ILog logger = HockeyLogManager.GetLog(typeof(UpdateManager));

        private static readonly UpdateManager instance = new UpdateManager();

        static UpdateManager() { }
        private UpdateManager() { }

        internal static UpdateManager Current
        {
            get { return instance; }
        }

        /// <summary>
        /// Check for an update on the server
        /// HockecCient needs to be configured before calling this method (normally done by configuring a crahshandler in the App() constructor)
        /// </summary>
        /// <param name="settings">[optional] custom settings</param>
        internal async Task RunUpdateCheckAsync(UpdateCheckSettings settings = null)
        {
            await UpdateVersionIfAvailable(settings ?? UpdateCheckSettings.DefaultStartupSettings);
        }

        internal async Task UpdateVersionIfAvailable(UpdateCheckSettings updateCheckSettings)
        {
            if (CheckWithUpdateFrequency(updateCheckSettings.UpdateCheckFrequency) && NetworkInterface.GetIsNetworkAvailable())
            {
                Exception thrownException = null;
                try
                {
                    var currentVersion = new Version(HockeyClient.Current.AsInternal().VersionInfo);
                    var appVersions = await HockeyClient.Current.AsInternal().GetAppVersionsAsync();
                    var newestAvailableAppVersion = appVersions.FirstOrDefault();
                    
                    if (appVersions.Any()
                        && new Version(newestAvailableAppVersion.Version) > currentVersion
                        && (updateCheckSettings.CustomDoShowUpdateFunc == null || updateCheckSettings.CustomDoShowUpdateFunc(newestAvailableAppVersion)))
                    {
                        if (updateCheckSettings.UpdateMode.Equals(UpdateMode.InApp) || (updateCheckSettings.EnforceUpdateIfMandatory && newestAvailableAppVersion.Mandatory))
                        {
                            NavigateToUpdatePage(currentVersion, appVersions, updateCheckSettings);
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
                            await (new MessageDialog(LocalizedStrings.LocalizedResources.NoUpdateAvailable).ShowAsync());
                        }
                    }
                }
                catch (Exception e)
                {
                    thrownException = e;
                    logger.Error(e);
                }
                //Don't show errors durgin update-check on startup
                if (thrownException != null && updateCheckSettings.UpdateMode.Equals(UpdateMode.InApp))
                {
                    await CoreWindow.GetForCurrentThread().Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () =>
                    {
                        await new MessageDialog(LocalizedStrings.LocalizedResources.UpdateUnknownError).ShowAsync();
                    });
                }
            }
        }

        internal bool CheckWithUpdateFrequency(UpdateCheckFrequency frequency)
        {
            //TODO implement. store and check last update timestamp...
            return true;
        }

        internal void NavigateToUpdatePage(Version currentVersion, IEnumerable<IAppVersion> appVersions, UpdateCheckSettings updateCheckSettings)
        {
            var rootFrame = Window.Current.Content as Frame;
            dynamic parms = new DynamicNavigationParameters();
            parms.currentVersion = currentVersion;
            parms.appVersions = appVersions;
            parms.updateCheckSettings = updateCheckSettings;
            
            rootFrame.Navigate(typeof(UpdatePage), parms);
        }

        protected async void ShowUpdateNotification(Version currentVersion, IEnumerable<IAppVersion> appVersions, UpdateCheckSettings updateCheckSettings)
        {
            var dialog = new MessageDialog(LocalizedStrings.LocalizedResources.UpdateAvailable, LocalizedStrings.LocalizedResources.UpdateNotification);

            dialog.Commands.Add(new UICommand(LocalizedStrings.LocalizedResources.Show, null, true));
            dialog.Commands.Add(new UICommand(LocalizedStrings.LocalizedResources.Dismiss, null, false));

            await CoreWindow.GetForCurrentThread().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => {
                var result = await dialog.ShowAsync();
                if ((bool)result.Id)
                {
                    NavigateToUpdatePage(currentVersion, appVersions, updateCheckSettings);
                }
            });
        }

        internal async void DoUpdate(IAppVersion availableUpdate)
        {
            var aetxUri = new Uri(HockeyClient.Current.AsInternal().ApiBaseVersion2 + "apps/" + HockeyClient.Current.AsInternal().AppIdentifier + ".aetx", UriKind.Absolute);
            var downloadUri = new Uri(HockeyClient.Current.AsInternal().ApiBaseVersion2 + "apps/" + HockeyClient.Current.AsInternal().AppIdentifier + "/app_versions/" + availableUpdate.Id + ".xap", UriKind.Absolute);

            //it won't get the result anyway because this app-instance will get killed during the update
            var result = await InstallationManager.AddPackageAsync(availableUpdate.Title, downloadUri);

        }
    }
}
