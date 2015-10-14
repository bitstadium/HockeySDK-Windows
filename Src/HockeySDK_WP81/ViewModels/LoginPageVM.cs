using Microsoft.HockeyApp.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.UI.Popups;

namespace Microsoft.HockeyApp.ViewModels
{
    public class LoginPageVM : VMBase
    {
        private string email;
        public string Email
        {
            get { return email; }
            set
            {
                email = value;
                NotifyOfPropertyChange("Email");
            }
        }

        public string AppSecret { get; set; }
        public AuthenticationMode AuthMode { get; set; }

        public bool IsAuthorize { get { return AuthenticationMode.Authorize.Equals(this.AuthMode); } }
        public bool IsIdentify { get { return AuthenticationMode.Identify.Equals(this.AuthMode); } }

        public string HeaderText
        {
            get
            {
                return AuthenticationMode.Authorize.Equals(this.AuthMode) ?
                         LocalizedStrings.LocalizedResources.AuthAuthorizeNote as String
                         : LocalizedStrings.LocalizedResources.AuthIdentifyNote as String;
            }
        }

        public string LoginButtonText
        {
            get
            {
                return AuthenticationMode.Authorize.Equals(this.AuthMode) ?
                         LocalizedStrings.LocalizedResources.AuthAuthorizeButton as String
                         : LocalizedStrings.LocalizedResources.AuthIdentifyButton as String;
            }
        }

        public async Task<IAuthStatus> IdentifyUserAsync()
        {
            IAuthStatus status = null;
            WebTransferException webEx = null;
            try
            {
                IsBusy = true;
                status = await HockeyClient.Current.AsInternal().IdentifyUserAsync(this.Email, this.AppSecret);
                if (status.IsIdentified)
                {
                    await AuthManager.Current.UpdateAuthStatusAsync(status);
                }
            }
            catch (WebTransferException wte)
            {
                webEx = wte;
            }
            finally
            {
                IsBusy = false;
            }
            if (webEx != null) { await HandleNetworkError(webEx); }
            return status;
        }

        internal async Task HandleNetworkError(WebTransferException wte) {
            await new MessageDialog(LocalizedStrings.LocalizedResources.AuthNoInternet).ShowAsync();
        }

        public async Task<IAuthStatus> AuthorizeUserAsync(string password)
        {
            IAuthStatus status = null;
            WebTransferException webEx = null;
            try
            {
                IsBusy = true;
                status = await HockeyClient.Current.AsInternal().AuthorizeUserAsync(this.Email, password ?? "");
                if (status.IsAuthorized)
                {
                    await AuthManager.Current.UpdateAuthStatusAsync(status);
                }
            }
            catch (WebTransferException wte)
            {
                webEx = wte;
            }

            finally
            {
                IsBusy = false;
            }
            if (webEx != null) { await HandleNetworkError(webEx); }
            return status;
        }
    }
}
