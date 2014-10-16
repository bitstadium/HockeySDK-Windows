using HockeyApp.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace HockeyApp
{
    public static class HockeyClientWPFExtensions
    {
        internal static IHockeyClientInternal AsInternal(this IHockeyClient @this)
        {
            return (IHockeyClientInternal)@this;
        }

        #region Configure

        public static IHockeyClientConfigurable Configure(this IHockeyClient @this, string identifier)
        {
            @this.AsInternal().PlatformHelper = new HockeyPlatformHelperWPF();
            @this.AsInternal().AppIdentifier = identifier;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            return (IHockeyClientConfigurable)@this;
        }

        private static Action<UnhandledExceptionEventArgs> customUnhandledExceptionAction;
        private static Action<UnobservedTaskExceptionEventArgs> customUnobservedTaskExceptionAction;
        private static Action<DispatcherUnhandledExceptionEventArgs> customDispatcherUnhandledExceptionAction;

        /// <summary>
        /// This will run after HockeyApp has written the crash-log to disk.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="customAction"></param>
        /// <returns></returns>
        public static IHockeyClientConfigurable RegisterCustomUnhandledExceptionLogic(this IHockeyClientConfigurable @this, Action<UnhandledExceptionEventArgs> customAction)
        {
            customUnhandledExceptionAction = customAction;
            return @this;
        }

        /// <summary>
        /// This will run after HockeyApp has written the crash-log to disk.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="customAction"></param>
        /// <returns></returns>
        public static IHockeyClientConfigurable RegisterCustomUnobserveredTaskExceptionLogic(this IHockeyClientConfigurable @this, Action<UnobservedTaskExceptionEventArgs> customAction)
        {
            customUnobservedTaskExceptionAction = customAction;
            return @this;
        }

        /// <summary>
        /// This will run after HockeyApp has written the crash-log to disk.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="customAction"></param>
        /// <returns></returns>
        public static IHockeyClientConfigurable RegisterCustomDispatcherUnhandledExceptionLogic(this IHockeyClientConfigurable @this, Action<DispatcherUnhandledExceptionEventArgs> customAction)
        {
            customDispatcherUnhandledExceptionAction = customAction;
            return @this;
        }


        /// <summary>
        /// Provide a custom resource manager to override standard sdk i18n strings
        /// </summary>
        /// <param name="this"></param>
        /// <param name="manager">resource manager to use</param>
        /// <returns></returns>
        public static IHockeyClientConfigurable UseCustomResourceManager(this IHockeyClientConfigurable @this, ResourceManager manager)
        {
            LocalizedStrings.CustomResourceManager = manager;
            return @this;
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HockeyClient.Current.AsInternal().HandleException((Exception)e.ExceptionObject);
            if (customUnhandledExceptionAction != null)
            {
                customUnhandledExceptionAction(e);
            }
        }

        static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            HockeyClient.Current.AsInternal().HandleException(e.Exception);
            if (customUnobservedTaskExceptionAction != null)
            {
                customUnobservedTaskExceptionAction(e);
            }
        }

        static void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            HockeyClient.Current.AsInternal().HandleException(e.Exception);
            if (customDispatcherUnhandledExceptionAction != null)
            {
                customDispatcherUnhandledExceptionAction(e);
            }
        }

        #endregion

        #region Update

        
        /// <summary>
        /// Check for available updates asynchronously.
        /// </summary>
        /// <param name="autoShowUi">Use the default update dialogs</param>
        /// <param name="shutdownRequest">Callback to gracefully stop your application. If using default-ui, call has to be provided.</param>
        /// <param name="updateAvailableAction">Callback for available versions, if you want to provide own update dialogs</param>
        public static async Task CheckForUpdatesAsync(this IHockeyClient @this, bool autoShowUi, Func<bool> shutdownActions = null, Action<IAppVersion> updateAvailableAction = null)
        {
            @this.AsInternal().CheckForInitialization();
            //TODO refactor for next version
            await HockeyClientWPF.Instance.UpdateManager.CheckForUpdatesAsync(autoShowUi, shutdownActions, updateAvailableAction);
        }

        #endregion

        #region Feedback
        /// <summary>
        /// Creates a new Feedback-Thread. Thread is stored on the server with the first message.
        /// </summary>
        /// <returns></returns>
        public static IFeedbackThread CreateFeedbackThread(this IHockeyClient @this)
        {
            return @this.AsInternal().CreateNewFeedbackThread();
        }

        /// <summary>
        /// Opens a Feedback-Thread on the server.
        /// </summary>
        /// <param name="feedbackToken">A guid which identifies the Feedback-Thread</param>
        /// <returns>The Feedback-Thread or, if not found or delete, null.</returns>
        public static async Task<IFeedbackThread> OpenFeedbackThreadAsync(this IHockeyClient @this,string feedbackToken)
        {
            return await @this.AsInternal().OpenFeedbackThreadAsync(feedbackToken);
        }
        #endregion

        #region CrashHandling

        /// <summary>
        /// Send crashes to the HockeyApp server. If crashes are available a messagebox will popoup to ask the user if he wants to send crashes.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="sendAutomatically">if true crashes will be sent without asking</param>
        /// <returns></returns>
        public static async Task<bool> SendCrashesAsync(this IHockeyClient @this, Boolean sendAutomatically = false)
        {
            @this.AsInternal().CheckForInitialization();

            if (sendAutomatically)
            {
                return await @this.AsInternal().SendCrashesAndDeleteAfterwardsAsync().ConfigureAwait(false);
            }
            else
            {
                if (await @this.AsInternal().AnyCrashesAvailableAsync())
                {
                    MessageBoxResult result = MessageBox.Show(LocalizedStrings.LocalizedResources.CrashLogQuestion, LocalizedStrings.LocalizedResources.CrashLogMessageBox, MessageBoxButton.YesNo);
                    if (result.Equals(MessageBoxResult.Yes))
                    {
                        return await @this.AsInternal().SendCrashesAndDeleteAfterwardsAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        await @this.AsInternal().DeleteAllCrashesAsync().ConfigureAwait(false);
                    }
                }
                return false;
            }
        }

        #endregion
    }
}
