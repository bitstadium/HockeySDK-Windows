using Microsoft.HockeyApp.Common;
using Microsoft.HockeyApp.Tools;
using Microsoft.HockeyApp.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace Microsoft.HockeyApp.Views
{
    public sealed partial class UpdatePage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        public UpdatePage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            this.DataContext = new UpdatePageVM();

        }

        public UpdatePageVM VM { get { return this.DataContext as UpdatePageVM; } }

        private void InstallOverApi_Click(object sender, RoutedEventArgs e)
        {
            this.Overlay.Visibility = Visibility.Visible;
            UpdateManager.Current.DoUpdate(this.VM.NewestVersion);
        }

        private async void InstallAETX_Click(object sender, RoutedEventArgs e)
        {
            var aetxUri = new Uri(HockeyClient.Current.AsInternal().ApiBaseVersion2 + "apps/" + this.VM.NewestVersion.PublicIdentifier + ".aetx", UriKind.Absolute);
            await Launcher.LaunchUriAsync(aetxUri);
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }


        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
            dynamic parms = e.Parameter as DynamicNavigationParameters;

            IEnumerable<IAppVersion> appVersions = parms.appVersions;

            this.VM.NewestVersion = appVersions.First();

            this.AppIconImage.ImageFailed += (sender, ex) => { 
                this.AppIconImage.Source = new BitmapImage(new Uri("ms-appx:///" + WebBrowserHelper.AssemblyNameWithoutExtension + "/Assets/windows_phone.png", UriKind.RelativeOrAbsolute)); 
            };
            this.AppIconImage.Source = new BitmapImage(new Uri(HockeyClient.Current.AsInternal().ApiBaseVersion2 + "apps/" + this.VM.NewestVersion.PublicIdentifier + ".png"));

            this.ReleaseNotesBrowser.NavigateToString(await WebBrowserHelper.WrapContentAsync(this.VM.NewestVersion.Notes));
            this.ReleaseNotesBrowser.NavigationStarting += async (sender, navEventArgs) => {
                
                if (navEventArgs.Uri != null)
                {
                    navEventArgs.Cancel = true;
                    await Launcher.LaunchUriAsync(navEventArgs.Uri);
                }
            };
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

    }
}
