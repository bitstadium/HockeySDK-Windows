using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HockeyApp.ViewModels
{
    public class LoginPageVM : VMBase
    {
        public string Email { get; set; }
        public string AppSecret { get; set; }
        public string Password { get; set; }
        public AuthenticationMode AuthMode { get; set; }

        public bool IsAuthorize {get{ return AuthenticationMode.Authorize.Equals(this.AuthMode);}}
        public bool IsIdentify { get { return AuthenticationMode.Identify.Equals(this.AuthMode); } }

        public string HeaderText
        {
            get
            {
                return AuthenticationMode.Authorize.Equals(this.AuthMode) ?
                         LocalizedStrings.LocalizedResources.Authorize as String
                         : LocalizedStrings.LocalizedResources.Identify as String;

            }
        }

        public string LoginButtonText
        {
            get
            {
                return AuthenticationMode.Authorize.Equals(this.AuthMode) ?
                         LocalizedStrings.LocalizedResources.Authorize as String
                         : LocalizedStrings.LocalizedResources.Identify as String;
            }
        }

        public bool IsShowOverlay { get; set; }

        internal async Task<bool> IdentifyUser()
        {
            try
            {
                IsShowOverlay = true;
                IAuthStatus status = await HockeyClient.Instance.IdentifyUser(this.Email, this.AppSecret);
                if (status.IsIdentified)
                {
                    AuthManager.Instance.CurrentAuthStatus = status;
                    IsShowOverlay = false;
                    return true;
                }
                else {
                    IsShowOverlay = false;
                    //TODO Fehler anzeigen!!
                    return false; 
                }
            }
            catch (Exception)
            {
                IsShowOverlay = false;
                throw;
            }
        }

        internal async Task<bool> AuthorizeUser(string password)
        {
            try
            {
                IsShowOverlay = true;
                IAuthStatus status = await HockeyClient.Instance.AuthorizeUser(this.Email, password ?? "");
                if (status.IsAuthorized)
                {
                    AuthManager.Instance.CurrentAuthStatus = status;
                    IsShowOverlay = false;
                    return true;
                }
                else {
                    IsShowOverlay = false;
                    return false; }
            }
            catch (Exception)
            {
                IsShowOverlay = false;
                throw;
            }
        }

        
    }
}
