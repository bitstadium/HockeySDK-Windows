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
using Windows.UI.Core;
using Windows.UI.Popups;
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
    public sealed partial class FeedbackMainPage : Page
    {
        private NavigationHelper navigationHelper;

        internal FeedbackThreadVM VM { get { return this.DataContext as FeedbackThreadVM; } }

        public FeedbackMainPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
            if (Frame.BackStack.Any() && Frame.BackStack.Last().SourcePageType == this.GetType())
            {
                Frame.BackStack.Remove(Frame.BackStack.Last());
            }
            var pageStackEntry = Frame.BackStack.LastOrDefault(entry => entry.SourcePageType == this.GetType());
            if (pageStackEntry != null) { Frame.BackStack.Remove(pageStackEntry); }

            this.DataContext = await FeedbackManager.Current.GetDataContextNavigatedToListPage(e, MessageList);
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }
    }
}
