using HockeyApp.Internal;
using HockeyApp.Tools;
using HockeyApp.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace HockeyApp
{
    public static class HockeyClientExtensionsWP81
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

            Application.Current.Resuming += HandleAppResuming;
            Application.Current.Suspending += HandleAppSuspending;

            Application.Current.UnhandledException += async (sender, e) => { 
                e.Handled = true;
                await HockeyClient.Current.AsInternal().HandleExceptionAsync(e.Exception);
                Application.Current.Exit();
            };
            return @this as IHockeyClientConfigurable;
        }

        static async void HandleAppSuspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            await FeedbackManager.Current.StoreDataIfNeeded();
            deferral.Complete();
        }

        static void HandleAppResuming(object sender, object e)
        {
            
        }


        #endregion

        #region Update

        /// <summary>
        /// Call this method during startup of your app to check for Updates.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="updateSettings">Settings to define context for Update checking. </param>
        /// <returns></returns>
        public static async Task CheckForAppUpdateAsync(this IHockeyClient @this, UpdateCheckSettings updateSettings = null)
        {
            HockeyClient.Current.AsInternal().CheckForInitialization();
            await UpdateManager.Current.RunUpdateCheckAsync(updateSettings).ConfigureAwait(false);
        }

        #endregion


        #region Feedback

        /// <summary>
        /// Invoke this method to navigate to the feedback UI where a user can send you a message including image attachments over the HockeyApp feedback system.
        /// Make sure to add a call to HockeyClient.Current.HandleReactivationOfFeedbackFilePicker(..) to your App's OnActivated() method to allow for resuming 
        /// the process after PickFileAndContinue..
        /// </summary>
        /// <param name="this"></param>
        /// <param name="initialUsername">[Optional] Username to prefill the name field</param>
        /// <param name="initialEmail">[Optional] Email to prefill the email field</param>
        public static void NavigateToFeedbackPage(this IHockeyClient @this, string initialUsername = null, string initialEmail = null)
        {
            HockeyClient.Current.AsInternal().CheckForInitialization();

            dynamic pars = new DynamicNavigationParameters();
            pars.IsCallFromApp = true;
            FeedbackManager.Current.InitialEmail = initialEmail;
            FeedbackManager.Current.InitialUsername = initialUsername;
            var frame = Window.Current.Content as Frame;
            frame.Navigate(typeof(FeedbackMainPage), pars);
        }

        /// <summary>
        /// You need to call this method in your App's OnActivated method if you use the feedback feature. This allows for HockeyApp to continue after a
        /// PickFileAndContinue resume when adding images as attachments to a message
        /// </summary>
        /// <param name="this"></param>
        /// <param name="e"></param>
        /// <returns>true if the reactivation occured because of Feedback Filepicker</returns>
        public static bool HandleReactivationOfFeedbackFilePicker(this IHockeyClient @this, IActivatedEventArgs e)
        {
            var continuationEventArgs = e as IContinuationActivatedEventArgs;
            if (continuationEventArgs != null && ActivationKind.PickFileContinuation.Equals(continuationEventArgs.Kind))
            {
                var args = (FileOpenPickerContinuationEventArgs)e;
                if (args.ContinuationData.ContainsKey(FeedbackManager.FilePickerContinuationKey))
                {
                    if (args.Files.Count > 0)
                    {
                        dynamic pars = new DynamicNavigationParameters();
                        pars.ImageFile = args.Files[0];
                        ((Window.Current.Content) as Frame).Navigate(typeof(FeedbackImagePage), pars);
                        return true;
                    }
                    else
                    {
                        ((Window.Current.Content) as Frame).Navigate(typeof(FeedbackFormPage));
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Call in your app if user logs out. Deletes all persistently stored data like FeedbackThreadToken, and cached Message. .
        /// </summary>
        public static async Task LogoutFromFeedbackAsync(this IHockeyClient @this)
        {
            var settingValues = ApplicationData.Current.LocalSettings.Values;
            settingValues.RemoveValue(ConstantsUniversal.FeedbackThreadKey);
            await FeedbackManager.Current.ClearMessageCacheAsync();
        }

        #endregion


    }
}
