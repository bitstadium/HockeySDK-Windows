using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HockeyApp.Tools;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using HockeyApp.Views;

namespace HockeyApp
{
    internal partial class AuthManager
    {
        protected void ShowLoginScreen(AuthenticationMode authMode, string appSecret, string email, AuthValidationMode authValidationMode)
        {
            dynamic parms = new DynamicNavigationParameters();
            parms.authmode = authMode;
            parms.appsecret = appSecret;
            parms.email = email;
            parms.validationmode = authValidationMode;
            this.Frame.Navigate(typeof(LoginPage), parms);
        }
    
    }
}
