namespace Microsoft.HockeyApp.Extensibility.Windows
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    
    using Channel;
    using DataContracts;
    using Extensibility;

#if WINRT
    using global::Windows.UI.Xaml;
#endif

    /// <summary>
    /// A module that deals in Exception events and will create ExceptionTelemetry objects when triggered.
    /// </summary>
    public sealed partial class UnhandledExceptionTelemetryModule : ITelemetryModule, IDisposable
    {
        private Task initialized;
        private TelemetryClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnhandledExceptionTelemetryModule"/> class.
        /// </summary>
        internal UnhandledExceptionTelemetryModule()
        {
        }
        
        internal bool AlwaysHandleExceptions { get; set; }

        internal Task Initialized
        {
            get { return this.initialized; }
        }
        
        /// <summary>
        /// Unsubscribe from the <see cref="Application.UnhandledException"/> event.
        /// </summary>
        public void Dispose()
        {
            PlatformDispatcher.RunAsync(() => Application.Current.UnhandledException -= this.ApplicationOnUnhandledException).Wait();
        }

        /// <summary>
        /// Subscribes to unhandled event notifications.
        /// </summary>
        public void Initialize(TelemetryConfiguration configuration)
        {
            LazyInitializer.EnsureInitialized(ref this.initialized, () => this.InitializeAsync());
        }
        
        /// <summary>
        /// Issues with the previous code - 
        /// We were changing the exception as handled which should not be done, 
        /// as the application might want the exception in other unhandled exception event handler.
        /// Re throw of the exception triggers the users unhandled exception event handler twice and also caused the infinite loop issue.
        /// Creating a new thread is not a good practice and the code will eventually move to persist and send exception on resume as hockeyApp.
        /// </summary>
        internal void ApplicationOnUnhandledException(object sender, object e)
        {
            LazyInitializer.EnsureInitialized(ref this.client, this.CreateClient);
#if WINRT
            UnhandledExceptionEventArgs args = (UnhandledExceptionEventArgs)e;
            Exception eventException = args.Exception;
#elif SILVERLIGHT
            ApplicationUnhandledExceptionEventArgs args = (ApplicationUnhandledExceptionEventArgs)e;
            Exception eventException = args.ExceptionObject;
#endif
            var exceptionTelemetry = new ExceptionTelemetry(eventException);
            exceptionTelemetry.HandledAt = ExceptionHandledAt.Unhandled;

            this.client.TrackException(exceptionTelemetry);
            this.client.Flush();
        }

        private TelemetryClient CreateClient()
        {
            TelemetryClient client = new TelemetryClient(TelemetryConfiguration.Active);
            client.Channel = new InMemoryChannel();

            string endpoints = TelemetryConfiguration.Active.TelemetryChannel.EndpointAddress;
            if (!string.IsNullOrEmpty(endpoints))
            {
                client.Channel.EndpointAddress = endpoints;
            }

            return client;
        }
    }
}
