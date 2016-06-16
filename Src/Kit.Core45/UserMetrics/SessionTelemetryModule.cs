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

    using Services;
    /// <summary>
    /// Tracks user sessions for Store Apps (Windows Store and Windows Phone).
    /// </summary>
    internal sealed class SessionTelemetryModule : ITelemetryInitializer, ITelemetryModule
    {
        private const string SessionIdSetting = "HockeyAppSessionId";
        private const string SessionEndSetting = "HockeyAppSessionEnd";

        private readonly IPlatformService platform;
        private readonly IClock clock;
        private bool isFirstSession;
        private string sessionId;
        private TimeSpan timeout = TimeSpan.FromSeconds(20);
        private IApplicationService application;


        /// <summary>
        /// Initializes a new instance of the <see cref="SessionTelemetryModule"/> class.
        /// </summary>
        internal SessionTelemetryModule() : this(PlatformSingleton.Current, Clock.Instance)
        {
        }

        internal SessionTelemetryModule(IPlatformService platform, IClock clock)
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
        public void Initialize()
        {
            // Avoid double initialization (once as a TelemetryModule and once as a TelemetryInitializer)
            // To track SessionStateTelemetry, this module handles Windows Phone lifecycle events.
            application = ServiceLocator.GetService<IApplicationService>();
            application.Init();

            // ToDo: Clarify what to do with Silverlight applications.
//#if SILVERLIGHT
//                PhoneApplicationService.Current.Activated += this.HandleApplicationStartedEvent;
//                PhoneApplicationService.Current.Launching += this.HandleApplicationStartedEvent;

//                PhoneApplicationService.Current.Deactivated += this.HandleApplicationStoppingEvent;
//                PhoneApplicationService.Current.Closing += this.HandleApplicationStoppingEvent;
//#endif
            application.OnResuming += this.HandleApplicationStartedEvent; 
            application.OnSuspending += this.HandleApplicationStoppingEvent; 

            // To set Session.Id for all telemetry types, this module also serves as a TelemetryInitializer.
            TelemetryConfiguration.Active.TelemetryInitializers.Add(this);

            this.TrackSessionState();
            this.SaveSessionState(); // To prevent HandleApplicationStarted from tracking a duplicate session start
        }

        internal async void HandleApplicationStartedEvent(object sender, object e)
        {
            // Resuming event is running on the UI thread. TrackSessionState() can take long time to run, which can show 'Resuming...' message.
            // We are forcing to run it on a different thread to prevent 'Resuming..' message to be shown and for app to resume faster.
            await System.Threading.Tasks.Task.Run(() => { TrackSessionState(); }).ConfigureAwait(false);
        }

        internal void HandleApplicationStoppingEvent(object sender, object e)
        {
            this.SaveSessionState();
        }

        private bool GetPreviousSession(out string previousSessionId, out DateTimeOffset previousSessionEnd)
        {
            IDictionary<string, object> settings = this.platform.GetLocalApplicationSettings();

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
            IDictionary<string, object> settings = this.platform.GetLocalApplicationSettings();
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

            if (application.IsDevelopmentMode())
            {
                // In emulator mode we don't send a user as a new user ever. Because if we do - during onboarding experience new users chart will show
                // more users than total users. ToDo: do better new user detection on on server side: If we see that user is coming from emulator, we need to 
                // set its isFirst flag only the first time on the server.
                this.isFirstSession = false;
            }

            this.sessionId = Guid.NewGuid().ToString();

            this.Track(SessionState.Start, this.sessionId, this.clock.Time);

            // Without calling TelemetryClient.Flush we have up to 50 delay for the user to see the data (30 sec interval to write to temp file, 10 sec interval to read data from the file)
            // As (1) this is important statistic, it tracks users and session and (2) it is important onboarding experience, calling Flush to remove 30 sec write interval.
            // ToDo: Investigate whether it affects performance.
            ((HockeyClient)HockeyClient.Current).Flush();
        }

        private void Track(SessionState state, string id, DateTimeOffset timestamp)
        {
            var session = new SessionStateTelemetry(state);
            session.Context.Session.Id = id;
            session.Timestamp = timestamp;
            ((HockeyClient)HockeyClient.Current).Track(session);
        }
    }
}
