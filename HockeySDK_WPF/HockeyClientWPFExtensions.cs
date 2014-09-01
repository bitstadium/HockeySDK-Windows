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

        public static IHockeyClientConfigurable Configure(this IHockeyClient @this, string identifier, bool keepRunningAfterException)
        {
            @this.AsInternal().PlatformHelper = new HockeyPlatformHelperWPF();
            @this.AsInternal().AppIdentifier = identifier;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            if (keepRunningAfterException)
            {
                Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            }
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            return (IHockeyClientConfigurable)@this;
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

        static async void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            await HockeyClient.Current.AsInternal().HandleExceptionAsync(e.Exception);
            e.Handled = true;
        }

        #region CrashHandling

        //TODO docu
           /// <summary>
           /// 
           /// </summary>
           /// <param name="this"></param>
           /// <param name="sendAutomatically"></param>
           /// <returns></returns>
        public static async Task<bool> HandleCrashesAsync(this IHockeyClient @this, Boolean sendAutomatically = false)
        {
            @this.AsInternal().CheckForInitialization();
            return await @this.AsInternal().SendCrashesAndDeleteAfterwardsAsync().ConfigureAwait(false);
        }

        #endregion

    }
}
