using HockeyApp.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace HockeyApp.Views
{
    public sealed partial class FeedbackFlyout : SettingsFlyout
    {
        public FeedbackFlyout()
        {
            this.InitializeComponent();
            var vm = FeedbackManager.Current.CurrentFeedbackFlyoutVM;
            vm.CalledFromNewFlyout(this);
            this.DataContext = vm;

        }

        bool closeRequested = false;

        protected override async void OnGotFocus(RoutedEventArgs e)
        {
            await (this.DataContext as FeedbackFlyoutVM).InitializeIfNeededAsync();
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            if (!closeRequested)
            {
                this.ShowIndependent();
            }
        }

        private void SettingsFlyout_BackClick(object sender, BackClickEventArgs e)
        {
            closeRequested = true;
            this.Hide();
        }

        

    }
}
