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

namespace HockeyApp.Views
{
    public partial class LoginPage : PhoneApplicationPage
    {
        internal LoginPageVM VM
        {
            get { return (this.DataContext as LoginPageVM); }
            private set { this.DataContext = value; }
        }

        protected async Task AuthenticateOnline()
        {
            SystemTray.ProgressIndicator.IsVisible = true;
            try
            {
                if (await (this.VM.IsAuthorize ? this.VM.AuthorizeUser(this.Password.Password) : this.VM.IdentifyUser()))
                {
                    NavigationService.Navigate(AuthManager.Instance.SuccessRedirect);
                    return;
                }
                else
                {
                    NavigationService.Navigate(AuthManager.Instance.FailRedirect);
                }
            } //TODO error handling
            catch (Exception) { throw; }
            finally { SystemTray.ProgressIndicator.IsVisible = false; }
        }

        public LoginPage()
        {
            this.VM = new LoginPageVM();
            InitializeComponent();
            Login.Click += async (sender, ev) =>
            {
                await AuthenticateOnline();
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

            CheckForExistingLogin(authValidationMode);
        }

        protected async void CheckForExistingLogin(AuthValidationMode authValidationMode)
        {
            string serializedAuthStatus = AuthManager.Instance.RetrieveProtectedString(Constants.AuthStatusKey);
            if (!String.IsNullOrEmpty(serializedAuthStatus))
            {
                var aS = AuthStatus.DeserializeFromString(serializedAuthStatus);
                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    try
                    {
                        SystemTray.ProgressIndicator.IsVisible = true;
                        if (await aS.CheckIfStillValid())
                        {
                            AuthManager.Instance.CurrentAuthStatus = aS;
                            NavigationService.Navigate(AuthManager.Instance.SuccessRedirect);
                        }
                    }
                    catch (Exception)
                    {
                        
                        if (AuthValidationMode.Graceful.Equals(authValidationMode))
                        {
                            NavigationService.Navigate(AuthManager.Instance.SuccessRedirect); 
                        }
                        else
                        {
                            throw;
                        }
                    }
                    finally
                    {
                        SystemTray.ProgressIndicator.IsVisible = false;
                    }
                }
                else
                {
                    if (AuthValidationMode.Graceful.Equals(authValidationMode))
                    {
                        NavigationService.Navigate(AuthManager.Instance.SuccessRedirect);
                    }
                   //TODO messagebox ?!?
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