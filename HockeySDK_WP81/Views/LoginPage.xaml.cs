using HockeyApp.Common;
using HockeyApp.Exceptions;
using HockeyApp.Model;
using HockeyApp.Tools;
using HockeyApp.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace HockeyApp.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        private LoginPageVM defaultViewModel = new LoginPageVM();

        public LoginPage()
        {
            this.DataContext = new LoginPageVM();
            this.InitializeComponent();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            await AuthenticateOnlineAsync();
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public LoginPageVM DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        internal LoginPageVM VM
        {
            get { return (this.DataContext as LoginPageVM); }
        }

        internal async Task AuthenticateOnlineAsync()
        {
            this.VM.IsBusy = true;
            Exception thrownException = null;
            try
            {
                IAuthStatus status = await (this.VM.IsAuthorize ? this.VM.AuthorizeUserAsync(this.Password.Password) : this.VM.IdentifyUserAsync());
                if (status.IsIdentified)
                {
                    AuthManager.Current.ExecuteSuccessRedirectOrAction();
                    return;
                }
                else
                {
                    if (status.IsCredentialError)
                    {
                        this.Password.Password = String.Empty;
                        await new MessageDialog(LocalizedStrings.LocalizedResources.AuthCredentialsError).ShowAsync();
                    }
                    else if (status.IsPermissionError)
                    {
                        this.Password.Password = String.Empty;
                        await new MessageDialog(LocalizedStrings.LocalizedResources.AuthNoMemberError).ShowAsync();
                    }
                    else
                    {
                        this.Password.Password = String.Empty;
                        await new MessageDialog(LocalizedStrings.LocalizedResources.AuthUnknownError).ShowAsync();
                    }
                }
            }
            catch (Exception e)
            {
                thrownException = e;
            }
            finally
            {
                this.VM.IsBusy = false;
            }
            if (thrownException != null)
            {
                if (thrownException is HockeyApp.Exceptions.WebTransferException)
                {
                    await new MessageDialog(LocalizedStrings.LocalizedResources.AuthNetworkError).ShowAsync();
                }
                else
                {
                    await new MessageDialog(LocalizedStrings.LocalizedResources.AuthUnknownError).ShowAsync();
                }
            }

        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            dynamic pars = e.Parameter as DynamicNavigationParameters ?? new DynamicNavigationParameters();

            this.VM.AuthMode = (AuthenticationMode?)pars.authmode ?? AuthenticationMode.Authorize;
            this.VM.AppSecret = (String)pars.appsecret ?? "";
            this.VM.Email = (String)pars.email ?? "";
            AuthValidationMode authValidationMode  = (AuthValidationMode?)pars.validationmode ?? AuthValidationMode.Graceful;

            this.VM.IsBusy = true;
            await AuthManager.Current.CheckAndHandleExistingTokenAsync(this.VM.AuthMode, authValidationMode);
            this.VM.IsBusy = false;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            // Remove current page from history
            var pageStackEntry = Frame.BackStack.LastOrDefault(entry => entry.SourcePageType == this.GetType());
            if (pageStackEntry != null)
            {
                Frame.BackStack.Remove(pageStackEntry);
            }
        }

    }
}
