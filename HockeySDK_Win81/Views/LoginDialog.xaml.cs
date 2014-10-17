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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace HockeyApp.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginDialog : Page
    {
        public LoginDialog()
        {
            this.InitializeComponent();
            var bounds = Window.Current.Bounds;
            this.RootPanel.Width = bounds.Width;
            this.RootPanel.Height = bounds.Height;
        }

        public event EventHandler CloseRequested;

        public LoginVM VM { get { return this.DataContext as LoginVM; } }


        internal async void OnOpened(AuthenticationMode authmode, AuthValidationMode validationMode)
        {
            VM.IsBusy = true;
            if (await AuthManager.Current.CheckAndHandleExistingTokenAsync(authmode, validationMode))
            {
                this.CloseRequested(this, EventArgs.Empty);
            }
            VM.IsBusy = false;
        }

        private async void IdentifyButton_Click(object sender, RoutedEventArgs e)
        {
            var hideMe = await this.VM.AuthorizeOnline();
            this.Password.Password = String.Empty;
            if (hideMe && this.CloseRequested != null) {
                this.CloseRequested(this, EventArgs.Empty);
            }
        }

        private async void AuthorizeButton_Click(object sender, RoutedEventArgs e)
        {
            var hideMe = await this.VM.AuthorizeOnline(this.Password.Password);
            this.Password.Password = String.Empty;
            if (hideMe && this.CloseRequested != null)
            {
                this.CloseRequested(this, EventArgs.Empty);
            }
        }

    }
}
