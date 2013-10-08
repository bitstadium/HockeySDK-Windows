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
using HockeyApp.Resources;

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
        public UpdateMode UpdateMode
        {
            get { return updateMode; }
            set { updateMode = value; }
        }
        
        private UpdateCheckFrequency updateCheckFrequency = UpdateCheckFrequency.Always;
        public UpdateCheckFrequency UpdateCheckFrequency
        {
            get { return updateCheckFrequency; }
            set { updateCheckFrequency = value; }
        }
        
        private Func<AppVersion,bool> customDoShowUpdateFunc = (version) => true;
        public Func<AppVersion,bool> CustomDoShowUpdateFunc
        {
            get { return customDoShowUpdateFunc; }
            set { customDoShowUpdateFunc = value; }
        }

        private bool enforceUpdateIfMandatory = true;
        public bool EnforceUpdateIfMandatory
        {
            get { return enforceUpdateIfMandatory; }
            set { enforceUpdateIfMandatory = value; }
        }

    }

    public class UpdateManager
    {

        private static readonly UpdateManager instance = new UpdateManager();
        private string identifier = null;

        static UpdateManager() { }
        private UpdateManager() { }

        public static UpdateManager Instance
        {
            get
            {
                return instance;
            }
        }

        public static void RunUpdateCheck(string identifier, UpdateCheckSettings settings = null)
        {
            Instance.identifier = identifier;
            Instance.UpdateVersionIfAvailable(settings ?? UpdateCheckSettings.DefaultStartupSettings);
        }

        internal void UpdateVersionIfAvailable(UpdateCheckSettings updateCheckSettings)
        {
            var request = WebRequest.CreateHttp(new Uri(Constants.ApiBase + "apps/" + identifier + ".json", UriKind.Absolute));
            request.Method = HttpMethod.Get;
            request.Headers[HttpRequestHeader.UserAgent] = Constants.UserAgentString;

            if (CheckWithUpdateFrequency(updateCheckSettings.UpdateCheckFrequency) && NetworkInterface.GetIsNetworkAvailable())
            {
                try
                {
                    var responseTask = request.GetResponseTaskAsync();
                    responseTask.ContinueWith((webResponseTask) =>
                    {
                        var response = webResponseTask.Result;
                        IEnumerable<AppVersion> appVersions = AppVersion.FromJson(response.GetResponseStream());
                        var newestAvailableAppVersion = appVersions.FirstOrDefault();
                        var currentVersion = new Version(ManifestHelper.GetAppVersion());
                        if (newestAvailableAppVersion != null
                            && new Version(newestAvailableAppVersion.version) > currentVersion
                            && updateCheckSettings.CustomDoShowUpdateFunc(newestAvailableAppVersion))
                        {
                            if (updateCheckSettings.UpdateMode.Equals(UpdateMode.InApp) || (updateCheckSettings.EnforceUpdateIfMandatory && newestAvailableAppVersion.mandatory))
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
                                Scheduler.Dispatcher.Schedule(() => MessageBox.Show(SdkResources.NoUpdateAvailable));
                            }
                        }
                    }, TaskContinuationOptions.NotOnFaulted);
                }
                catch (Exception e)
                {
                }
            }
        }


        internal bool CheckWithUpdateFrequency(UpdateCheckFrequency frequency)
        {
            //TODO implement. store and check last update timestamp...
            return true;
        }

        protected void ShowUpdateNotification(Version currentVersion, IEnumerable<AppVersion> appVersions, UpdateCheckSettings updateCheckSettings)
        {
            Scheduler.Dispatcher.Schedule(() =>
            {
                NotificationTool.Show(
                    SdkResources.UpdateNotification,
                    SdkResources.UpdateAvailable,
                    new NotificationAction(SdkResources.Show, () =>
                    {
                        ShowVersionPopup(currentVersion, appVersions, updateCheckSettings);
                    }),
                    new NotificationAction(SdkResources.Dismiss, () =>
                    {
                        //DO nothing
                    })
                );
            });
        }

        protected void ShowVersionPopup(Version currentVersion, IEnumerable<AppVersion> appVersions, UpdateCheckSettings updateCheckSettings)
        {
            Scheduler.Dispatcher.Schedule(() =>
            {
                appVersions.First().PublicIdentifier = this.identifier;
                //TODO hooks for customizing
                UpdatePopupTool.ShowPopup(currentVersion, appVersions, updateCheckSettings, DoUpdate);
            });
        }

        internal async void DoUpdate(AppVersion availableUpdate)
        {
            var aetxUri = new Uri(Constants.ApiBase + "apps/" + this.identifier + ".aetx", UriKind.Absolute);
            var downloadUri = new Uri(Constants.ApiBase + "apps/" + this.identifier + "/app_versions/" + availableUpdate.id + ".xap", UriKind.Absolute);

            //it won't get the result anyway because htis app-instance will get killed during the update
            await InstallationManager.AddPackageAsync(availableUpdate.title, downloadUri);
        }
    }
}
