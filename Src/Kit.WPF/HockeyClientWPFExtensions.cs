using Microsoft.HockeyApp.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.HockeyApp.Extensibility;
using Microsoft.HockeyApp.Services;

namespace Microsoft.HockeyApp
{
    /// <summary>
    /// HockeyClientWPFExtensions class.
    /// </summary>
    public static class HockeyClientWPFExtensions
    {
        private static IUpdateManager _updateManager = null;

        private static HashSet<Exception> _processedExceptions = new HashSet<Exception>();

        public static IUpdateManager UpdateManager
        {
            get
            {
                if (_updateManager == null)
                {
                    _updateManager = new UpdateManager();
                }
                return _updateManager;
            }
        }

        #region Configure

        /// <summary>
        /// Configures the client.
        /// </summary>
        /// <param name="this">This object.</param>
        /// <param name="identifier">Identifier.</param>
        /// <returns>HockeyClient configurable.</returns>
        public static IHockeyClientConfigurable Configure(this IHockeyClient @this, string identifier)
        {
            if (@this.AsInternal().TestAndSetIsConfigured())
            {
                return @this as IHockeyClientConfigurable;
            }

            @this.AsInternal().AppIdentifier = identifier;
            @this.AsInternal().PlatformHelper = new HockeyPlatformHelperWPF();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            ServiceLocator.AddService<IPlatformService>(new PlatformService());
            TelemetryConfiguration.Active.InstrumentationKey = identifier;
            
            return (IHockeyClientConfigurable)@this;
        }

        private static Action<UnhandledExceptionEventArgs> customUnhandledExceptionAction;
        private static Action<UnobservedTaskExceptionEventArgs> customUnobservedTaskExceptionAction;
        private static Action<DispatcherUnhandledExceptionEventArgs> customDispatcherUnhandledExceptionAction;

        /// <summary>
        /// Adds the handler for UnobservedTaskExceptions
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static IHockeyClientConfigurable RegisterDefaultUnobservedTaskExceptionHandler(this IHockeyClientConfigurable @this)
        {
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            return @this;
        }

        /// <summary>
        /// Removes the handler for UnobservedTaskExceptions
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static IHockeyClientConfigurable UnregisterDefaultUnobservedTaskExceptionHandler(this IHockeyClientConfigurable @this)
        {
            TaskScheduler.UnobservedTaskException -= TaskScheduler_UnobservedTaskException;
            return @this;
        }

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
            var exception = (Exception)e.ExceptionObject;
            if (_processedExceptions.Contains(exception))
            {
                // Handles the case where multiple handlers are called for the same exception.
                return;
            }
            _processedExceptions.Add(exception);

            HockeyClient.Current.AsInternal().HandleException(exception);
            if (customUnhandledExceptionAction != null)
            {
                customUnhandledExceptionAction(e);
            }
        }

        static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            if (_processedExceptions.Contains(e.Exception))
            {
                // Handles the case where multiple handlers are called for the same exception.
                return;
            }
            _processedExceptions.Add(e.Exception);

            HockeyClient.Current.AsInternal().HandleException(e.Exception);
            if (customUnobservedTaskExceptionAction != null)
            {
                customUnobservedTaskExceptionAction(e);
            }
        }

        static void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (_processedExceptions.Contains(e.Exception))
            {
                // Handles the case where multiple handlers are called for the same exception.
                return;
            }
            _processedExceptions.Add(e.Exception);

            HockeyClient.Current.AsInternal().HandleException(e.Exception);
            if (customDispatcherUnhandledExceptionAction != null)
            {
                customDispatcherUnhandledExceptionAction(e);
            }
        }

        #endregion

        #region Update

#pragma warning disable 612, 618
        /// <summary>
        /// Check for available updates asynchronously.
        /// </summary>
        /// <param name="this">The this.</param>
        /// <param name="autoShowUi">Use the default update dialogs</param>
        /// <param name="shutdownActions">Callback to gracefully stop your application. If using default-ui, call has to be provided.</param>
        /// <param name="updateAvailableAction">Callback for available versions, if you want to provide own update dialogs</param>
        /// <returns></returns>
        public static async Task<bool> CheckForUpdatesAsync(this IHockeyClient @this, bool autoShowUi, Func<bool> shutdownActions = null, Action<IAppVersion> updateAvailableAction = null)
        {
            @this.AsInternal().CheckForInitialization();
            return await UpdateManager.CheckForUpdatesAsync(autoShowUi, shutdownActions, updateAvailableAction);
        }
#pragma warning restore 612, 618

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
        /// <param name="this">The this.</param>
        /// <param name="feedbackToken">A guid which identifies the Feedback-Thread</param>
        /// <returns>
        /// The Feedback-Thread or, if not found or delete, null.
        /// </returns>
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

        #region Helper

        private static string _appIdHash = null;

        /// <summary>
        /// Gets the AppId hash.
        /// </summary>
        public static string AppIdHash
        {
            get {
                if (_appIdHash == null)
                {
                    _appIdHash = GetMD5Hash(HockeyClient.Current.AsInternal().AppIdentifier);
                }
                return _appIdHash; }
        }

        internal static string GetMD5Hash(string sourceString)
        {
            if (String.IsNullOrEmpty(sourceString)) { return string.Empty; }
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] sourceBytes = Encoding.Default.GetBytes(sourceString);
            byte[] result = md5.ComputeHash(sourceBytes);
            return System.BitConverter.ToString(result);
        }

        #endregion
    }
}
