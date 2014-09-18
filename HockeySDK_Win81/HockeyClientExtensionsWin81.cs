using HockeyApp.Internal;
using HockeyApp.ViewModels;
using HockeyApp.Tools;
using HockeyApp.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace HockeyApp
{
    public static class HockeyClientExtensionsWin81
    {

        #region Configure
        /// <summary>
        /// This is the main configuration method. Call this in the Constructor of your app. This registers an error handler for unhandled errors.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="appIdentifier">Your unique app id from HockeyApp.</param>
        /// <returns>Configurable Hockey client. Configure additional settings by calling methods on the returned IHockeyClientConfigurable</returns>
        public static IHockeyClientConfigurable Configure(this IHockeyClient @this, string appIdentifier)
        {
            @this.AsInternal().PlatformHelper = new HockeyPlatformHelper81();
            @this.AsInternal().AppIdentifier = appIdentifier;

            Application.Current.UnhandledException += async (sender, e) => { 
                e.Handled = true; 
                await HockeyClient.Current.AsInternal().HandleExceptionAsync(e.Exception);
                Application.Current.Exit();
            };
            
            return @this as IHockeyClientConfigurable;
        }
        #endregion

        #region Feedback

        /// <summary>
        /// -- COMING SOON ! NOT IMPLEMENTED YET --
        /// Invoke this method to open the feedback UI where a user can send you a message including image attachments over the HockeyApp feedback system.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="initialUsername">[Optional] Username to prefill the name field</param>
        /// <param name="initialEmail">[Optional] Email to prefill the email field</param>
        public static void ShowFeedbackPopup(this IHockeyClient @this, string initialEMail = null, string initialUserName = null)
        {
            //throw new NotImplementedException("Coming Soon! - Not yet implemented.");
            //TODO Feedback for Windows 81
            
            var flyout = new FeedbackFlyout();
            FeedbackManager.Current.InitialEmail = initialEMail;
            FeedbackManager.Current.InitialUsername = initialUserName;
            flyout.ShowIndependent();
            
        }

        /// <summary>
        /// Call in your app if user logs out. Deletes all persistently stored data like FeedbackThreadToken, and cached Message. .
        /// </summary>
        public static void LogoutFromFeedback(this IHockeyClient @this)
        {
            var settingValues = ApplicationData.Current.LocalSettings.Values;
            settingValues.RemoveValue(ConstantsUniversal.FeedbackThreadKey);
            FeedbackManager.Current.CurrentFeedbackFlyoutVM = new FeedbackFlyoutVM();
        }

        #endregion

    }
}
