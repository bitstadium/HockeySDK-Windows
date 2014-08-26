using HockeyApp.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace HockeyApp
{
    public static class HockeyClientWP8SLExtension
    {

        internal static IHockeyClientInternal AsInternal(this IHockeyClient @this)
        {
            return (IHockeyClientInternal)@this;
        }

        #region Configuration
        /// <summary>
        /// 
        /// </summary>
        /// <param name="this"></param>
        /// <param name="application"></param>
        /// <param name="appId"></param>
        /// <param name="rootFrame"></param>
        /// <returns></returns>
        public static IHockeyClientConfigurable Configure(this IHockeyClient @this, string appId, Frame rootFrame = null)
        {
            @this.AsInternal().PlatformHelper = new HockeyPlatformHelperWP8SL();
            @this.AsInternal().AppIdentifier = appId;
            CrashHandler.Current.Application = Application.Current;
            CrashHandler.Current.Application.UnhandledException += (sender, args) => { CrashHandler.Current.HandleException(args.ExceptionObject); };

            if (rootFrame != null)
            {
                //Idea based on http://www.markermetro.com/2013/01/technical/handling-unhandled-exceptions-with-asyncawait-on-windows-8-and-windows-phone-8/
                //catch async void Exceptions
                AsyncSynchronizationContext.RegisterForFrame(rootFrame, CrashHandler.Current);
            }

            return @this as IHockeyClientConfigurable;
        }

        public static IHockeyClientConfigurable UseCustomResourceManager(this IHockeyClientConfigurable @this, ResourceManager manager)
        {
            //TODO make LocalizedStrings.CustomResourceManager internal in next major version
            #pragma warning disable 0618
            LocalizedStrings.CustomResourceManager = manager;
            #pragma warning restore 0618
            return @this;
        }


        #endregion

        #region Wrappers for functions

        #region CrashHandling

        public static async Task<bool> HandleCrashesAsync(this IHockeyClient @this, Boolean sendAutomatically = false)
        {
            @this.AsInternal().CheckForInitialization();
            return await CrashHandler.Current.HandleCrashesAsync(sendAutomatically).ConfigureAwait(false);
        }

        #endregion

        #region Authentication
        public static void AuthorizeUser(this IHockeyClient @this, 
            Uri successRedirect, NavigationService navigationService,
            string eMail = null,
            TokenValidationPolicy tokenValidationPolicy = TokenValidationPolicy.EveryLogin,
            AuthValidationMode authValidationMode = AuthValidationMode.Graceful)

        {
            @this.AsInternal().CheckForInitialization();
            AuthManager.Instance.AuthenticateUser(navigationService,successRedirect, AuthenticationMode.Authorize, 
                tokenValidationPolicy, authValidationMode, eMail, null);
        }

        public static void IdentifyUser(this IHockeyClient @this, string appSecret,
            Uri successRedirect, NavigationService navigationService,
            string eMail = null,
            TokenValidationPolicy tokenValidationPolicy = TokenValidationPolicy.EveryLogin,
            AuthValidationMode authValidationMode = AuthValidationMode.Graceful)

        {
            @this.AsInternal().CheckForInitialization();
            AuthManager.Instance.AuthenticateUser(navigationService,successRedirect, AuthenticationMode.Authorize, 
                tokenValidationPolicy, authValidationMode, eMail, appSecret);
        }

        public static void LogoutUser(this IHockeyClient @this)
        {
            AuthManager.Instance.RemoveUserToken();
        }

        #endregion

        #region Feedback
        public static void ShowFeedbackUI(this IHockeyClient @this, NavigationService navigationService)
        {
            @this.AsInternal().CheckForInitialization();
            FeedbackManager.Instance.NavigateToFeedbackUI(navigationService);
        }

        #endregion

        #region Updates

        public static void CheckForUpdates(this IHockeyClient @this, UpdateCheckSettings settings = null)
        {
            @this.AsInternal().CheckForInitialization();
            UpdateManager.Instance.RunUpdateCheck(settings);
        }

        #endregion

        #endregion
    }
}
