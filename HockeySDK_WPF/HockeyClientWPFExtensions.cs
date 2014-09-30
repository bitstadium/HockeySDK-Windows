using HockeyApp.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
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
            HockeyClient.Current.AsInternal().HandleExceptionAsync(e.Exception);
            if (customUnobservedTaskExceptionAction != null)
            {
                customUnobservedTaskExceptionAction(e);
            }
        }

        static void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            HockeyClient.Current.AsInternal().HandleExceptionAsync(e.Exception);
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
        //TODO
        #endregion

        #region CrashHandling

        /// <summary>
        /// Send crashes to the HockeyApp server
        /// </summary>
        /// <param name="this"></param>
        /// <param name="sendAutomatically"></param>
        /// <returns></returns>
        public static async Task<bool> SendCrashesAsync(this IHockeyClient @this, Boolean sendAutomatically = false)
        {
            @this.AsInternal().CheckForInitialization();
            return await @this.AsInternal().SendCrashesAndDeleteAfterwardsAsync().ConfigureAwait(false);
        }

        #endregion

    }
}
