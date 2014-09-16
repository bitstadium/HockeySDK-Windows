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
        private NavigationHelper navigationHelper;
        private LoginPageVM defaultViewModel = new LoginPageVM();

        public LoginPage()
        {
            this.DataContext = new LoginPageVM();
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            await AuthenticateOnlineAsync();
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public LoginPageVM DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {

        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {

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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            dynamic pars = e.Parameter as DynamicNavigationParameters ?? new DynamicNavigationParameters();

            this.VM.AuthMode = (AuthenticationMode?)pars.authmode ?? AuthenticationMode.Authorize;
            this.VM.AppSecret = (String)pars.appsecret ?? "";
            this.VM.Email = (String)pars.email ?? "";
            AuthValidationMode authValidationMode  = (AuthValidationMode?)pars.validationmode ?? AuthValidationMode.Graceful;

            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            this.navigationHelper.OnNavigatedFrom(e);

            // Remove current page from history
            var pageStackEntry = Frame.BackStack.LastOrDefault(entry => entry.SourcePageType == this.GetType());
            if (pageStackEntry != null)
            {
                Frame.BackStack.Remove(pageStackEntry);
            }
        }

    }
}
