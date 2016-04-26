namespace Microsoft.HockeyApp
{
    using Channel;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Send information to the HockeyApp service.
    /// </summary>
    public class HockeyClient : IHockeyClient
    {
        /// <summary>
        /// Laziness implementation of a singleton
        /// </summary>
        private static readonly Lazy<HockeyClient> lazy = new Lazy<HockeyClient>(() => new HockeyClient());

        /// <summary>
        /// Telemetry buffer of items that were tracked by the user before <see cref="telemetryClient"/> instance has been created.
        /// </summary>
        private readonly Queue<string> eventQueue = new Queue<string>();

        private TelemetryClient telemetryClient;

        /// <summary>
        /// Gets the current singleton instance of HockeyClient.
        /// </summary>
        public static HockeyClient Current
        {
            get { return lazy.Value; }
        }

        /// <summary>
        /// Initializes <see cref="telemetryClient"/>. 
        /// For performance reasons, this call needs to be performed only after <see cref="TelemetryConfiguration"/> has been initialized.
        /// </summary>
        internal void Initialize()
        {
            this.telemetryClient = new TelemetryClient();
            TrackQueuedTelemetry();
        }

        /// <summary>
        /// Processes telemetry that was sent before <see cref="telemetryClient"/> instance has been initialized
        /// </summary>
        internal void TrackQueuedTelemetry()
        {
            while (eventQueue.Count > 0)
            {
                this.telemetryClient.TrackEvent(eventQueue.Dequeue());
            }
        }

        /// <summary>
        /// Bootstraps HockeyApp SDK.
        /// </summary>
        /// <param name="appId">The application identifier, which is a unique hash string which is automatically created when you add a new application to HockeyApp.</param>
        public void Configure(string appId)
        {
            WindowsAppInitializer.InitializeAsync(appId, null);
        }

        /// <summary>
        /// Bootstraps HockeyApp SDK.
        /// </summary>
        /// <param name="appId">The application identifier, which is a unique hash string which is automatically created when you add a new application to HockeyApp.</param>
        /// <param name="configuration">Telemetry Configuration.</param>
        public void Configure(string appId, TelemetryConfiguration configuration)
        {
            ServiceLocator.AddService<StorageBase>(new Storage());
            WindowsAppInitializer.InitializeAsync(appId, configuration);
        }

        /// <summary>
        /// Send a custom event for display in Events tab.
        /// </summary>
        /// <param name="eventName">Event name</param>
        public void TrackEvent(string eventName)
        {
            if (this.telemetryClient != null)
            {
                this.telemetryClient.TrackEvent(eventName);
            } else
            {
                eventQueue.Enqueue(eventName);
            }
        }
    }
}
