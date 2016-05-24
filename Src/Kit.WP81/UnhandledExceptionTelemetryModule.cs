namespace Microsoft.HockeyApp
{
    using System;
    using Services;
    using Windows.UI.Xaml;

    internal class UnhandledExceptionTelemetryModule : IUnhandledExceptionTelemetryModule
    {
        private bool initialized;

        internal static Func<UnhandledExceptionEventArgs, bool> CustomUnhandledExceptionFunc
        {
            get; set;
        }

        public void Initialize(TelemetryConfiguration configuration)
        {
            if (!initialized)
            {
                Application.Current.UnhandledException += async (sender, e) =>
                {
                    e.Handled = true;
                    await HockeyClient.Current.AsInternal().HandleExceptionAsync(e.Exception);
                    if (CustomUnhandledExceptionFunc == null || CustomUnhandledExceptionFunc(e))
                    {
                        Application.Current.Exit();
                    }
                };

                initialized = true;
            }
        }
    }
}
