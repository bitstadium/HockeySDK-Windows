namespace Microsoft.HockeyApp.Services
{
    using System;
    using System.Windows.Forms;
    using System.Threading;

    using Channel;

    using DataContracts;

    internal sealed class UnhandledExceptionTelemetryModule : IUnhandledExceptionTelemetryModule
    {
        private bool initialized;

        internal UnhandledExceptionTelemetryModule(bool keepRunningAfterException)
        {
            if (keepRunningAfterException)
            {
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            }
        }

        public void Initialize()
        {
            if (!initialized)
            {
                Application.ThreadException += Application_ThreadException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                initialized = true;
            }
        }
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            if (ex != null)
            {
                HockeyClient.Current.AsInternal().HandleException(ex);
            }
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            HockeyClient.Current.AsInternal().HandleException(e.Exception);
        }

        public ITelemetry CreateCrashTelemetry(Exception exception, ExceptionHandledAt handledAt)
        {
            return new ExceptionTelemetry(exception) { HandledAt = handledAt };
        }
    }
}
