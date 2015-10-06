using HockeyApp.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HockeyPlatformHelperWinForms = HockeyApp.HockeyPlatformHelperWPF;
using Microsoft.ApplicationInsights;

namespace HockeyApp
{
    /// <summary>
    /// HockeyClient Extension for WinForms.
    /// </summary>
    public static class HockeyClientWinFormsExtensions
    {
        internal static IHockeyClientInternal AsInternal(this IHockeyClient @this)
        {
            return (IHockeyClientInternal)@this;
        }

        /// <summary>
        /// Configures HockeyClient.
        /// </summary>
        /// <param name="this">HockeyClient object.</param>
        /// <param name="identifier">Identfier.</param>
        /// <param name="keepRunningAfterException">Keep running after exception.</param>
        /// <returns>Instance object.</returns>
        public static IHockeyClientConfigurable Configure(this IHockeyClient @this, string identifier, bool keepRunningAfterException)
        {
            @this.AsInternal().PlatformHelper = new HockeyPlatformHelperWinForms();
            @this.AsInternal().AppIdentifier = identifier;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            if (keepRunningAfterException)
            {
                Application.ThreadException += Current_ThreadException;
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            }

            Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration.Active.InstrumentationKey = identifier;
            return (IHockeyClientConfigurable)@this;
        }

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

        static async void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            if (ex != null)
            {
                await HockeyClient.Current.AsInternal().HandleExceptionAsync(ex);
            }
        }
        static async void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            await HockeyClient.Current.AsInternal().HandleExceptionAsync(e.Exception);
        }

        static async void Current_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            await HockeyClient.Current.AsInternal().HandleExceptionAsync(e.Exception);
        }

        #region CrashHandling

        /// <summary>
        /// Send crashes to the HockeyApp server
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static async Task<bool> SendCrashesAsync(this IHockeyClient @this)
        {
            @this.AsInternal().CheckForInitialization();
            bool result = await @this.AsInternal().SendCrashesAndDeleteAfterwardsAsync().ConfigureAwait(false);
            return result;
        }

        #endregion

    }
}
