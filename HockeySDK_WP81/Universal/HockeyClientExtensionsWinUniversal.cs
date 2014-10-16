using HockeyApp.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HockeyApp
{
    /// <summary>
    /// Extensions for Win 8.1 Store Apps and Windows Phone 8.1 Store Apps
    /// </summary>
    public static class HockeyClientExtensionsWinUniversal
    {
        #region Function wrappers


        #region Crashes

        /// <summary>
        /// Send any collected crashes to the HockeyApp server. You should normally call this during startup of your app. 
        /// </summary>
        /// <param name="this"></param>
        /// <param name="sendWithoutAsking">configures if available crashes are sent immediately or if the user should be asked if the crashes should be sent or discarded</param>
        /// <returns>true if crashes where sent successfully</returns>
        public static async Task<bool> SendCrashesAsync(this IHockeyClient @this, bool sendWithoutAsking = false)
        {
            @this.AsInternal().CheckForInitialization();
            return await CrashHandler.Current.HandleCrashesAsync(sendWithoutAsking);
        }

        #endregion

        #region Authentication

        /// <summary>
        /// Inititate user authorization and define a action to perform when authorization is successfull
        /// </summary>
        /// <param name="this"></param>
        /// <param name="successAction">Action to perform when login is successfull</param>
        /// <param name="eMail">[Optional] E-Mail adress to prefill form</param>
        /// <param name="tokenValidationPolicy">[Optional] Default is EveryLogin</param>
        /// <param name="authValidationMode">[Optional] Default is Graceful</param>
        public static void AuthorizeUser(this IHockeyClient @this,
            Action successAction, string eMail = null,
            TokenValidationPolicy tokenValidationPolicy = TokenValidationPolicy.EveryLogin,
            AuthValidationMode authValidationMode = AuthValidationMode.Graceful)
        {
            @this.AsInternal().CheckForInitialization();
            var authMan = AuthManager.Current;
            authMan.SuccessAction = successAction;
            authMan.AuthenticateUser(AuthenticationMode.Authorize,
                tokenValidationPolicy, authValidationMode, eMail, null);
        }

        /// <summary>
        /// Inititate user identification and define a action to perform when authorization is successfull
        /// </summary>
        /// <param name="this"></param>
        /// <param name="successAction">Action to perform when login is successfull</param>
        /// <param name="eMail">[Optional] E-Mail adress to prefill form</param>
        /// <param name="tokenValidationPolicy">[Optional] Default is EveryLogin</param>
        /// <param name="authValidationMode">[Optional] Default is Graceful</param>
        public static void IdentifyUser(this IHockeyClient @this, string appSecret,
            Action successAction, string eMail = null,
            TokenValidationPolicy tokenValidationPolicy = TokenValidationPolicy.EveryLogin,
            AuthValidationMode authValidationMode = AuthValidationMode.Graceful)
        {
            @this.AsInternal().CheckForInitialization();
            var authMan = AuthManager.Current;
            authMan.SuccessAction = successAction;
            authMan.AuthenticateUser(AuthenticationMode.Identify,
                tokenValidationPolicy, authValidationMode, eMail, appSecret);
        }


        /// <summary>
        /// Inititate user authorization and define a page navigate to when authorization is successfull
        /// </summary>
        /// <param name="this"></param>
        /// <param name="pageTypeForSuccessRedirect">Pagetype to navigate when login is successfull</param>
        /// <param name="eMail">[Optional] E-Mail adress to prefill form</param>
        /// <param name="tokenValidationPolicy">[Optional] Default is EveryLogin</param>
        /// <param name="authValidationMode">[Optional] Default is Graceful</param>
        public static void AuthorizeUser(this IHockeyClient @this,
           Type pageTypeForSuccessRedirect, string eMail = null,
           TokenValidationPolicy tokenValidationPolicy = TokenValidationPolicy.EveryLogin,
           AuthValidationMode authValidationMode = AuthValidationMode.Graceful)
        {
            @this.AsInternal().CheckForInitialization();
            var authMan = AuthManager.Current;
            authMan.SuccessRedirectPageType = pageTypeForSuccessRedirect;
            AuthManager.Current.AuthenticateUser(AuthenticationMode.Authorize,
                tokenValidationPolicy, authValidationMode, eMail, null);
        }

        /// <summary>
        /// Inititate user identification and define a page to navigate to when authorization is successfull
        /// </summary>
        /// <param name="this"></param>
        /// <param name="appSecret">your app secret from HockeyApp</param>
        /// <param name="pageTypeForSuccessRedirect">Pagetype to navigate when login is successfull</param>
        /// <param name="eMail">[Optional] E-Mail adress to prefill form</param>
        /// <param name="tokenValidationPolicy">[Optional] Default is EveryLogin</param>
        /// <param name="authValidationMode">[Optional] Default is Graceful</param>
        public static void IdentifyUser(this IHockeyClient @this, string appSecret,
            Type pageTypeForSuccessRedirect,
            string eMail = null,
            TokenValidationPolicy tokenValidationPolicy = TokenValidationPolicy.EveryLogin,
            AuthValidationMode authValidationMode = AuthValidationMode.Graceful)
        {
            @this.AsInternal().CheckForInitialization();
            var authMan = AuthManager.Current;
            authMan.SuccessRedirectPageType = pageTypeForSuccessRedirect;
            AuthManager.Current.AuthenticateUser(AuthenticationMode.Identify,
                tokenValidationPolicy, authValidationMode, eMail, appSecret);
        }

        /// <summary>
        /// Delete an already obtained user-token if existing
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static async Task LogoutUserAsync(this IHockeyClient @this)
        {
            await AuthManager.Current.RemoveUserTokenAsync();
        }

        #endregion

        #region Internal

        internal static IHockeyClientInternal AsInternal(this IHockeyClient @this)
        {
            return (IHockeyClientInternal)@this;
        }

        internal static IHockeyClientInternal AsInternal(this IHockeyClientConfigurable @this)
        {
            return (IHockeyClientInternal)@this;
        }
        #endregion

        #endregion

    }
}
