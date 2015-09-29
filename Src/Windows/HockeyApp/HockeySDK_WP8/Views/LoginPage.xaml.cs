using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using HockeyApp.ViewModels;
using HockeyApp.Model;
using System.Threading.Tasks;
using Microsoft.Phone.Net.NetworkInformation;
using HockeyApp.Exceptions;

namespace HockeyApp.Views
{
    public partial class LoginPage : PhoneApplicationPage
    {
        internal LoginPageVM VM
        {
            get { return (this.DataContext as LoginPageVM); }
            private set { this.DataContext = value; }
        }

        protected async Task AuthenticateOnlineAsync()
        {
            SystemTray.ProgressIndicator.IsVisible = true;
            try
            {
                IAuthStatus status = await (this.VM.IsAuthorize ? this.VM.AuthorizeUserAsync(this.Password.Password) : this.VM.IdentifyUserAsync());
                if (status.IsIdentified)
                {
                    NavigationService.Navigate(AuthManager.Instance.SuccessRedirect);
                    return;
                }
                else
                {
                    if (status.IsCredentialError)
                    {
                        this.Password.Password = String.Empty;
                        MessageBox.Show(LocalizedStrings.LocalizedResources.AuthCredentialsError);
                    }
                    else if (status.IsPermissionError)
                    {
                        this.Password.Password = String.Empty;
                        MessageBox.Show(LocalizedStrings.LocalizedResources.AuthNoMemberError);
                    }
                    else
                    {
                        this.Password.Password = String.Empty;
                        MessageBox.Show(LocalizedStrings.LocalizedResources.AuthUnknownError);
                    }
                }
            }
            catch (Exception e)
            {
                if (e is HockeyApp.Exceptions.WebTransferException)
                {
                    MessageBox.Show(LocalizedStrings.LocalizedResources.AuthNetworkError);
                }
                else
                {
                    MessageBox.Show(LocalizedStrings.LocalizedResources.AuthUnknownError);
                }
            }
            finally
            {
                SystemTray.ProgressIndicator.IsVisible = false;
            }
        }

        public LoginPage()
        {
            this.VM = new LoginPageVM();
            InitializeComponent();
            Login.Click += async (sender, ev) =>
            {
                await AuthenticateOnlineAsync();
            };
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string authmode = "";
            string appsecret = "";
            string email = "";
            string validationmode = "";
            AuthValidationMode authValidationMode = AuthValidationMode.Graceful;

            if (NavigationContext.QueryString.TryGetValue("authmode", out authmode)) { this.VM.AuthMode = (AuthenticationMode)Enum.Parse(typeof(AuthenticationMode), authmode); }
            if (NavigationContext.QueryString.TryGetValue("appsecret", out appsecret)) { this.VM.AppSecret = appsecret; }
            if (NavigationContext.QueryString.TryGetValue("email", out email)) { this.VM.Email = email; }
            if (NavigationContext.QueryString.TryGetValue("validationmode", out validationmode)) { authValidationMode = (AuthValidationMode)Enum.Parse(typeof(AuthValidationMode), validationmode); }
            base.OnNavigatedTo(e);

            CheckForExistingLoginAsync(authValidationMode, this.VM.AuthMode);
        }

        protected async void CheckForExistingLoginAsync(AuthValidationMode authValidationMode, AuthenticationMode authMode)
        {
            string serializedAuthStatus = AuthManager.Instance.RetrieveProtectedString(Constants.AuthStatusKey);
            if (!String.IsNullOrEmpty(serializedAuthStatus))
            {
                var aS = AuthStatus.DeserializeFromString(serializedAuthStatus);
                //consider that a change in Authmode is possible between versions of an app, so check if the saved token may be trusted
                if (AuthenticationMode.Authorize.Equals(authMode) && !aS.IsAuthorized || AuthenticationMode.Identify.Equals(authMode) && aS.IsAuthorized)
                {
                    return;
                }

                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    this.VM.IsShowOverlay = true;
                    try
                    {
                        SystemTray.ProgressIndicator.IsVisible = true;
                        if (await aS.CheckIfStillValidAsync())
                        {
                            AuthManager.Instance.CurrentAuthStatus = aS;
                            NavigationService.Navigate(AuthManager.Instance.SuccessRedirect);
                        }
                        else
                        {
                            Dispatcher.BeginInvoke(() => { MessageBox.Show(LocalizedStrings.LocalizedResources.AuthNoMemberError); });
                        }
                    }
                    catch (WebTransferException)
                    {
                        
                        if (AuthValidationMode.Graceful.Equals(authValidationMode))
                        {
                            NavigationService.Navigate(AuthManager.Instance.SuccessRedirect); 
                        }
                        else
                        {
                            Dispatcher.BeginInvoke(() => { MessageBox.Show(LocalizedStrings.LocalizedResources.AuthNetworkError); });
                        }
                    }
                    finally
                    {
                        SystemTray.ProgressIndicator.IsVisible = false;
                        this.VM.IsShowOverlay = false;
                    }
                }
                else
                {
                    if (AuthValidationMode.Graceful.Equals(authValidationMode))
                    {
                        NavigationService.Navigate(AuthManager.Instance.SuccessRedirect);
                    }
                    Dispatcher.BeginInvoke(() => { MessageBox.Show(LocalizedStrings.LocalizedResources.AuthNetworkError); });
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.RemoveBackEntry();
            }
            base.OnNavigatedFrom(e);
        }

        protected override void OnOrientationChanged(OrientationChangedEventArgs e)
        {
            base.OnOrientationChanged(e);
        }

    }
}