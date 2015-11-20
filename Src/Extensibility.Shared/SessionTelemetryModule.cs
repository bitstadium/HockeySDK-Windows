namespace Microsoft.HockeyApp.Extensibility.Windows
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using Channel;
    using DataContracts;
    using Extensibility.Implementation;
    using Extensibility.Implementation.Platform;
#if SILVERLIGHT
    using Microsoft.Phone.Shell;
#endif
#if WINRT || WINDOWS_UWP
    using global::Windows.ApplicationModel.Core;
#endif

    /// <summary>
    /// Tracks user sessions for Store Apps (Windows Store and Windows Phone).
    /// </summary>
    internal sealed class SessionTelemetryModule : ITelemetryInitializer, ITelemetryModule
    {
        private const string SessionIdSetting = "HockeyAppSessionId";
        private const string SessionEndSetting = "HockeyAppSessionEnd";

        private readonly IPlatform platform;
        private readonly IClock clock;
        private bool isFirstSession;
        private string sessionId;
        private TimeSpan timeout = TimeSpan.FromSeconds(20);
        private TelemetryClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionTelemetryModule"/> class.
        /// </summary>
        internal SessionTelemetryModule() : this(PlatformSingleton.Current, Clock.Instance)
        {
        }

        internal SessionTelemetryModule(IPlatform platform, IClock clock)
        {
            Debug.Assert(platform != null, "platform");
            Debug.Assert(clock != null, "clock");
            this.platform = platform;
            this.clock = clock;
        }

        /// <summary>
        /// Gets or sets the value that determines if previous user session timed out.
        /// </summary>
        /// <remarks>
        /// Store apps can be suspended or even closed when user switches back and forth between apps. 
        /// If the amount of time between the moment an app is closed and then started again is less 
        /// than <see cref="Timeout"/> we assume that the previous session continues.
        /// </remarks>
        public TimeSpan Timeout 
        { 
            get
            {
                return this.timeout;
            }

            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                this.timeout = value;
            }
        }

        /// <summary>
        /// Initializes <see cref="SessionContext.Id"/> property of the <see cref="TelemetryContext.Session"/> context.
        /// </summary>
        void ITelemetryInitializer.Initialize(ITelemetry telemetry)
        {
            if (string.IsNullOrEmpty(telemetry.Context.Session.Id))
            {
                telemetry.Context.Session.Id = this.sessionId;
            }

            if (!telemetry.Context.Session.IsFirst.HasValue && this.isFirstSession)
            {
                telemetry.Context.Session.IsFirst = true;
            }
        }

        /// <summary>
        /// Initializes this instance of <see cref="SessionTelemetryModule"/> and begins session tracking.
        /// </summary>
        public void Initialize(TelemetryConfiguration configuration)
        {
            // Avoid double initialization (once as a TelemetryModule and once as a TelemetryInitializer)
            if (this.client == null)
            {
                // To track SessionStateTelemetry, this module handles Windows Phone lifecycle events.
                this.client = new TelemetryClient(configuration);
#if SILVERLIGHT
                PhoneApplicationService.Current.Activated += this.HandleApplicationStartedEvent;
                PhoneApplicationService.Current.Launching += this.HandleApplicationStartedEvent;

                PhoneApplicationService.Current.Deactivated += this.HandleApplicationStoppingEvent;
                PhoneApplicationService.Current.Closing += this.HandleApplicationStoppingEvent;
#endif
#if WINRT || WINDOWS_UWP
                CoreApplication.Resuming += this.HandleApplicationStartedEvent;
                CoreApplication.Suspending += this.HandleApplicationStoppingEvent;
#endif

                // To set Session.Id for all telemetry types, this module also serves as a TelemetryInitializer.
                configuration.TelemetryInitializers.Add(this);

                this.TrackSessionState();
                this.SaveSessionState(); // To prevent HandleApplicationStarted from tracking a duplicate session start
            }
        }

        internal void HandleApplicationStartedEvent(object sender, object e)
        {
            this.TrackSessionState();
        }

        internal void HandleApplicationStoppingEvent(object sender, object e)
        {
            this.SaveSessionState();
        }

        private bool GetPreviousSession(out string previousSessionId, out DateTimeOffset previousSessionEnd)
        {
            IDictionary<string, object> settings = this.platform.GetApplicationSettings();

            object storedSessionId;
            object storedSessionEnd;
            if (settings.TryGetValue(SessionIdSetting, out storedSessionId) &&
                settings.TryGetValue(SessionEndSetting, out storedSessionEnd))
            {
                previousSessionId = (string)storedSessionId;
                previousSessionEnd = DateTimeOffset.Parse((string)storedSessionEnd, CultureInfo.InvariantCulture);
                return true;
            }

            previousSessionId = null;
            return false;
        }
        
        private bool IsSessionActive(DateTimeOffset sessionEnd)
        {
            return (this.clock.Time - sessionEnd) < this.Timeout;
        }

        private void SaveSessionState()
        {
            IDictionary<string, object> settings = this.platform.GetApplicationSettings();
            settings[SessionIdSetting] = this.sessionId;
            settings[SessionEndSetting] = this.clock.Time.ToString("o", CultureInfo.InvariantCulture);
        }

        private void TrackSessionState()
        {
            string previousSessionId;
            DateTimeOffset previousSessionEnd;
            bool previousSessionFound = this.GetPreviousSession(out previousSessionId, out previousSessionEnd);
            if (previousSessionFound)
            {
                if (this.IsSessionActive(previousSessionEnd))
                {
                    this.sessionId = previousSessionId;
                    return;
                }

                this.Track(SessionState.End, previousSessionId, previousSessionEnd);
            }

            this.isFirstSession = !previousSessionFound;
            this.sessionId = Guid.NewGuid().ToString();

            this.Track(SessionState.Start, this.sessionId, this.clock.Time);
        }

        private void Track(SessionState state, string id, DateTimeOffset timestamp)
        {
            var session = new SessionStateTelemetry(state);
            session.Context.Session.Id = id;
            session.Timestamp = timestamp;
            this.client.Track(session);
        }
    }
}
