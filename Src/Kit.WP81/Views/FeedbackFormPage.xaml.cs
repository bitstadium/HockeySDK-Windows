using Microsoft.HockeyApp.Common;
using Microsoft.HockeyApp.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Microsoft.HockeyApp.Views
{
    public sealed partial class FeedbackFormPage : Page
    {
        private NavigationHelper navigationHelper;
        private FeedbackMessageVM defaultViewModel = new FeedbackMessageVM();

        public FeedbackFormPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
            StatusBar.GetForCurrentView().ForegroundColor = Colors.Black;
            await StatusBar.GetForCurrentView().ShowAsync();

            this.DataContext = await FeedbackManager.Current.GetDataContextNavigatedToFormPage(e);
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public FeedbackMessageVM DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

       
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

       
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            StatusBar.GetForCurrentView().ForegroundColor = null;
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion


    }
}
