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
using Microsoft.HockeyApp;

namespace HockeyApp
{
    /// <summary>
    /// Static extension class containing the main extension methods for controlling the HockeySDK client
    /// </summary>
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

            //Application.Current.Resuming += HandleAppResuming;
            Application.Current.Suspending += HandleAppSuspending;

            Application.Current.UnhandledException += async (sender, e) => { 
                e.Handled = true;
                await HockeyClient.Current.AsInternal().HandleExceptionAsync(e.Exception);
                if (customUnhandledExceptionFunc == null || customUnhandledExceptionFunc(e))
                {
                    Application.Current.Exit();
                }
            };

            WindowsAppInitializer.InitializeAsync(appIdentifier);
            return @this as IHockeyClientConfigurable;
        }

        private static Func<UnhandledExceptionEventArgs,bool> customUnhandledExceptionFunc;

        private static Func<UnobservedTaskExceptionEventArgs, bool> customUnobservedTaskExceptionFunc;

        /// <summary>
        /// Adds the handler for UnobservedTaskException
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static IHockeyClientConfigurable RegisterUnobservedTaskExceptionHandler(this IHockeyClientConfigurable @this)
        {
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            return @this;
        }

        /// <summary>
        /// Removes the handler for UnobservedTaskException
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static IHockeyClientConfigurable UnregisterUnobservedTaskExceptionHandler(this IHockeyClientConfigurable @this)
        {
            TaskScheduler.UnobservedTaskException -= TaskScheduler_UnobservedTaskException;
            return @this;
        }

        /// <summary>
        /// The func you set will be called after HockeyApp has written the crash-log and allows you to continue
        /// If the func returns false the app will not terminate but keep running
        /// </summary>
        /// <param name="this">The this.</param>
        /// <param name="customFunc">The custom function.</param>
        /// <returns></returns>
        public static IHockeyClientConfigurable RegisterCustomUnhandledExceptionLogic(this IHockeyClientConfigurable @this, Func<UnhandledExceptionEventArgs, bool> customFunc)
        {
            customUnhandledExceptionFunc = customFunc;
            return @this;
        }

        /// <summary>
        /// The func you set will be called after HockeyApp has written the crash-log and allows you to continue
        /// If the func returns false the app will not terminate but keep running
        /// </summary>
        /// <param name="this">The this.</param>
        /// <param name="customFunc">The custom function.</param>
        /// <returns></returns>
        public static IHockeyClientConfigurable RegisterCustomUnobserveredTaskExceptionLogic(this IHockeyClientConfigurable @this, Func<UnobservedTaskExceptionEventArgs, bool> customFunc)
        {
            customUnobservedTaskExceptionFunc = customFunc;
            return @this;
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
        static async void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            await HockeyClient.Current.AsInternal().HandleExceptionAsync(e.Exception);
            if (customUnobservedTaskExceptionFunc == null || customUnobservedTaskExceptionFunc(e))
            {
                Application.Current.Exit();
            }
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
        public static void ShowFeedback(this IHockeyClient @this, string initialUsername = null, string initialEmail = null)
        {
            HockeyClient.Current.AsInternal().CheckForInitialization();

            dynamic pars = new DynamicNavigationParameters();
            pars.IsCallFromApp = true;
            FeedbackManager.Current.InitialEmail = initialEmail;
            FeedbackManager.Current.InitialUsername = initialUsername;
            var frame = Window.Current.Content as Frame;
            frame.Navigate(typeof(FeedbackMainPage), pars);
        }

        [Obsolete("Use ShowFeedback() instead.")]
        public static void NavigateToFeedbackPage(this IHockeyClient @this, string initialUsername = null, string initialEmail = null)
        {
            @this.ShowFeedback(initialUsername, initialEmail);
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
