using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using HockeyApp.Tools;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using HockeyApp.Model;
using Microsoft.Phone.Reactive;
using System.IO;
using Windows.Storage.Streams;
using Windows.Storage;
using System.Windows.Media.Animation;
using Microsoft.Phone.Tasks;

namespace HockeyApp.Views
{
    public partial class AppUpdateControl : UserControl
    {
        public IAppVersion NewestVersion { get; set; }

        public AppUpdateControl(IEnumerable<IAppVersion> appVersions, Action<IAppVersion> updateAction)
        {
            this.NewestVersion = appVersions.First();
            InitializeComponent();

            this.AppIconImage.ImageFailed += (sender, e) => { this.AppIconImage.Source = new BitmapImage(new Uri("/Assets/windows_phone.png", UriKind.RelativeOrAbsolute)); };
            this.AppIconImage.Source = new BitmapImage(new Uri(HockeyClient.Instance.ApiBaseVersion2 + "apps/" + NewestVersion.PublicIdentifier + ".png"));

            this.ReleaseNotesBrowser.Opacity = 0;
            this.ReleaseNotesBrowser.Navigated += (sender, e) => { (this.ReleaseNotesBrowser.Resources["fadeIn"] as Storyboard).Begin(); };
            this.ReleaseNotesBrowser.NavigateToString(WebBrowserHelper.WrapContent(NewestVersion.Notes));

            this.InstallAETX.Click += (sender, e) =>
            {
                WebBrowserTask webBrowserTask = new WebBrowserTask();
                webBrowserTask.Uri = new Uri(HockeyClient.Instance.ApiBaseVersion2 + "apps/" + NewestVersion.PublicIdentifier + ".aetx", UriKind.Absolute);
                webBrowserTask.Show();
            };
            this.InstallOverApi.Click += (sender, e) => {
                this.Overlay.Visibility = Visibility.Visible;
                updateAction.Invoke(NewestVersion); 
            };
            
        }

    }
}