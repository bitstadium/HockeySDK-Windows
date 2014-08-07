using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HockeyApp.ViewModels
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

        private bool isShowOverlay;
        public bool IsShowOverlay
        {
            get { return isShowOverlay; }
            set
            {
                isShowOverlay = value;
                NotifyOfPropertyChange("IsShowOverlay");
            }
        }
        

        internal async Task<IAuthStatus> IdentifyUserAsync()
        {
            IAuthStatus status = null;
            try
            {
                IsShowOverlay = true;
                status = await HockeyClient.Current.AsInternal().IdentifyUserAsync(this.Email, this.AppSecret);
                if (status.IsIdentified)
                {
                    AuthManager.Instance.CurrentAuthStatus = status;
                }
            }
            finally
            {
                IsShowOverlay = false;
            }
            return status;
        }

        internal async Task<IAuthStatus> AuthorizeUserAsync(string password)
        {
            IAuthStatus status = null;
            try
            {
                IsShowOverlay = true;
                status = await HockeyClient.Current.AsInternal().AuthorizeUserAsync(this.Email, password ?? "");
                if (status.IsAuthorized)
                {
                    AuthManager.Instance.CurrentAuthStatus = status;
                }
            }
            finally
            {
                IsShowOverlay = false;
            }
            return status;
        }
    }
}
