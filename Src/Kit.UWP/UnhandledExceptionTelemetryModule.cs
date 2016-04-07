namespace Microsoft.HockeyApp.Extensibility.Windows
{
    using System;
    using System.Threading;
    
    using Channel;
    using DataContracts;
    using Extensibility;
    using Extensibility.Implementation.Tracing;

    using global::Windows.ApplicationModel.Core;
    using global::Windows.UI.Xaml;

    /// <summary>
    /// A module that deals in Exception events and will create ExceptionTelemetry objects when triggered.
    /// </summary>
    internal sealed partial class UnhandledExceptionTelemetryModule : ITelemetryModule, IDisposable
    {
        private TelemetryClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnhandledExceptionTelemetryModule"/> class.
        /// </summary>
        internal UnhandledExceptionTelemetryModule()
        {
        }
        
        internal bool AlwaysHandleExceptions { get; set; }
        
        /// <summary>
        /// Unsubscribe from the <see cref="Application.UnhandledException"/> event.
        /// </summary>
        public void Dispose()
        {
            CoreApplication.UnhandledErrorDetected -= CoreApplication_UnhandledErrorDetected;
        }

        /// <summary>
        /// Subscribes to unhandled event notifications.
        /// We are using <see cref="CoreApplication.UnhandledErrorDetected"/> instead of 
        /// <see cref="Application.UnhandledException"/> because <see cref="Application.UnhandledException"/> is not idempotent and 
        /// the exception object may be read only once. The second time it is read, it will return empty <see cref="System.Exception"/> without call stack.
        /// It is OS Bug 560663, 7133918 that must be fixed in Windows Redstone 2 (~2017).
        /// </summary>
        public void Initialize(TelemetryConfiguration configuration)
        {
            CoreApplication.UnhandledErrorDetected += CoreApplication_UnhandledErrorDetected;
        }

        private void CoreApplication_UnhandledErrorDetected(object sender, UnhandledErrorDetectedEventArgs e)
        {
            global::System.Diagnostics.Debug.WriteLine("UnhandledExceptionTelemetryModule.CoreApplication_UnhandledErrorDetected started successfully");
            try
            {
                // intentionally propagating exception to get the exception object that crashed the app.
                e.UnhandledError.Propagate();
            }
            catch (Exception eventException)
            {
                try
                {
                    LazyInitializer.EnsureInitialized(ref this.client, () => { return new TelemetryClient(); });
                    ITelemetry crashTelemetry = new CrashTelemetry(eventException) { HandledAt = ExceptionHandledAt.Unhandled };
                    this.client.Track(crashTelemetry);
                    this.client.Flush();
                }
                catch (Exception ex)
                {
                    CoreEventSource.Log.LogError("HockeySDK: An exeption occured in UnhandledExceptionTelemetryModule.CoreApplication_UnhandledErrorDetected: " + ex);
                }

                // if we don't throw exception - app will not be crashed. We need to throw to not change the app behavior.
                // known issue: stack trace will contain SDK methods from now on.
                throw;
            }
        }
    }
}
