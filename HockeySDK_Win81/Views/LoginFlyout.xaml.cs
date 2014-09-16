using HockeyApp.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace HockeyApp
{
    public sealed partial class LoginFlyout : SettingsFlyout
    {
        LoginFlyoutVM _vm { get { return this.DataContext as LoginFlyoutVM; } }
        
        internal LoginFlyout(LoginFlyoutVM initializedVM)
        {
            this.DataContext = initializedVM;
            this.InitializeComponent();
          //  this.BackClick += SettingsFlyoutItem_BackClick; this.Loaded += SettingsFlyoutItem_Loaded;
        }

        private async void IdentifyButton_Click(object sender, RoutedEventArgs e)
        {
            var hideMe = await this._vm.AuthorizeOnline();
            this.Password.Password = String.Empty;
            if (hideMe) { this.Hide(); }
            else { this.ShowIndependent(); }
        }

        private async void AuthorizeButton_Click(object sender, RoutedEventArgs e)
        {
            var hideMe = await this._vm.AuthorizeOnline(this.Password.Password);
            this.Password.Password = String.Empty;
            if (hideMe) { this.Hide(); }
            else { this.ShowIndependent(); }
        }

    }
}
