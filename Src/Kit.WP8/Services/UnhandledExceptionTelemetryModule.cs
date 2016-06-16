namespace Microsoft.HockeyApp
{
    using System;
    using System.Windows;
    using System.Windows.Controls;

    using Services;

    internal class UnhandledExceptionTelemetryModule : IUnhandledExceptionTelemetryModule
    {
        private readonly Frame rootFrame;
        private bool initialized;

        internal static Action<ApplicationUnhandledExceptionEventArgs> CustomUnhandledExceptionAction { get; set; }

        internal UnhandledExceptionTelemetryModule(Frame rootFrame)
        {
            this.rootFrame = rootFrame;
        }

        public void Initialize()
        {
            if (!initialized)
            {
                Application.Current.UnhandledException += (sender, args) =>
                {
                    CrashHandler.Current.HandleException(args.ExceptionObject);
                    if (CustomUnhandledExceptionAction != null)
                    {
                        CustomUnhandledExceptionAction(args);
                    }
                };

                if (rootFrame != null)
                {
                    //Idea based on http://www.markermetro.com/2013/01/technical/handling-unhandled-exceptions-with-asyncawait-on-windows-8-and-windows-phone-8/
                    //catch async void Exceptions

                    // set sync context for ui thread so async void exceptions can be handled, keeps process alive
                    AsyncSynchronizationContext.RegisterForFrame(rootFrame, CrashHandler.Current);
                }

                initialized = true;
            }
        }
    }
}
