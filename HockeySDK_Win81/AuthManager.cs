using HockeyApp.Tools;
using HockeyApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HockeyApp
{
    internal partial class AuthManager
    {

        protected void ShowLoginScreen(AuthenticationMode authMode, string appSecret, string email, AuthValidationMode authValidationMode)
        {
            var vm = new LoginFlyoutVM();
            vm.AuthMode = authMode;
            vm.Email = email;
            vm.AppSecret = appSecret;

            var flyout = new LoginFlyout(vm);
            flyout.ShowIndependent();
        }
    }
}
