using HockeyApp.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Popups;

namespace HockeyApp.ViewModels
{
    public class LoginFlyoutVM : VMBase
    {
        private ILog logger = HockeyLogManager.GetLog(typeof(LoginFlyoutVM));

        internal async Task<bool> AuthorizeOnline(string password = null)
        {
            IsBusy = true;
            Exception thrownException = null;
            try
            {
                IAuthStatus status = await (this.IsAuthorize ? this.AuthorizeUserAsync(password) : this.IdentifyUserAsync());
                if (status.IsIdentified)
                {
                    AuthManager.Current.ExecuteSuccessRedirectOrAction();
                    return true;
                }
                else
                {
                    if (status.IsCredentialError)
                    {
                        await new MessageDialog(LocalizedStrings.LocalizedResources.AuthCredentialsError).ShowAsync();
                    }
                    else if (status.IsPermissionError)
                    {
                        await new MessageDialog(LocalizedStrings.LocalizedResources.AuthNoMemberError).ShowAsync();
                    }
                    else
                    {
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
                IsBusy = false;
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
                    logger.Error(thrownException);
                }
            }
            return false;
        }


        public string FlyoutTitle
        {
            get { return LocalizedStrings.LocalizedResources.AuthFlyoutTitle; }
        }

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

        internal string AppSecret { get; set; }
        internal AuthenticationMode AuthMode { get; set; }

        public bool IsAuthorize { get { return AuthenticationMode.Authorize.Equals(this.AuthMode); } }
        public bool IsIdentify { get { return AuthenticationMode.Identify.Equals(this.AuthMode); } }


        internal async Task<IAuthStatus> IdentifyUserAsync()
        {
            IAuthStatus status = null;
            status = await HockeyClient.Current.AsInternal().IdentifyUserAsync(this.Email, this.AppSecret);
            if (status.IsIdentified)
            {
                await AuthManager.Current.UpdateAuthStatusAsync(status);
            }
            return status;
        }

        internal async Task<IAuthStatus> AuthorizeUserAsync(string password)
        {
            IAuthStatus status = null;
            status = await HockeyClient.Current.AsInternal().AuthorizeUserAsync(this.Email, password ?? "");
            if (status.IsAuthorized)
            {
                await AuthManager.Current.UpdateAuthStatusAsync(status);
            }
            return status;
        }

    }
}
