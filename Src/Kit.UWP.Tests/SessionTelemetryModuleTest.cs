namespace Microsoft.HockeyApp.Extensibility.Windows
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Channel;
    using DataContracts;
    using TestFramework;
    using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
    using Assert = Xunit.Assert;
    using Services;

    public class SessionTelemetryModuleTest
    {
        private readonly StubClock clock = new StubClock();
        private readonly Dictionary<string, object> settings = new Dictionary<string, object>();
        private readonly List<ITelemetry> sentTelemetry = new List<ITelemetry>();
        private readonly StubPlatform platform;
        private readonly StubTelemetryChannel channel;
        private readonly TelemetryConfiguration configuration;

        public SessionTelemetryModuleTest()
        {
            this.platform = new StubPlatform { OnGetApplicationSettings = () => this.settings };
            this.channel = new StubTelemetryChannel { OnSend = t => this.sentTelemetry.Add(t) };
            this.configuration = new TelemetryConfiguration { TelemetryChannel = this.channel, InstrumentationKey = "Test Key" };
            ServiceLocator.AddService<IApplicationService>(new ApplicationService());
            TelemetryConfiguration.Active = configuration;
            HockeyClient.Current.AsInternal().IsTelemetryInitialized = true;
        }

        private SessionTelemetryModule CreateSessionTelemetryModule()
        {
            return new SessionTelemetryModule(this.platform, this.clock);
        }

        [TestClass]
        public class Class : SessionTelemetryModuleTest
        {
            [TestMethod]
            public void ClassImplementsITelemetryInitializerBecauseNewSessionCanStartOnApplicationResume()
            {
                Assert.True(typeof(ITelemetryInitializer).IsAssignableFrom(typeof(SessionTelemetryModule)));
            }

            [TestMethod]
            public void ClassImplementsITelemetryModuleToGetHoldOfTelemetryConfigurationForTrackingSessionState()
            {
                Assert.True(typeof(ITelemetryModule).IsAssignableFrom(typeof(SessionTelemetryModule)));
            }
        }

        [TestClass]
        public class Timeout : SessionTelemetryModuleTest
        {
            [TestMethod]
            public void TimeoutValueIsAppropriateForMostStoreAppsByDefault()
            {
                SessionTelemetryModule initializer = this.CreateSessionTelemetryModule();
                Assert.Equal(TimeSpan.FromSeconds(20), initializer.Timeout);
            }

            [TestMethod]
            public void TimeoutSetterThrowsArgumentOutOfRangeExceptionWhenTimeoutIsLessThanZero()
            {
                SessionTelemetryModule initializer = this.CreateSessionTelemetryModule();
                Assert.Throws<ArgumentOutOfRangeException>(() => initializer.Timeout = TimeSpan.FromTicks(-1));
            }
        }

        [TestClass]
        public class InitializeITelemetry : SessionTelemetryModuleTest
        {
            [TestMethod]
            public void InitializeSetsSessionIdOfGivenTelemetryAfterApplicationIsStarted()
            {
                SessionTelemetryModule module = this.CreateSessionTelemetryModule();
                module.Initialize();                

                var telemetry = new StubTelemetry();
                ((ITelemetryInitializer)module).Initialize(telemetry);

                Assert.NotEmpty(telemetry.Context.Session.Id);
            }

            [TestMethod]
            public void InitializeDoesNotSetSessionIdOfGivenTelemetryBeforeApplicationIsStarted()
            {
                SessionTelemetryModule module = this.CreateSessionTelemetryModule();

                var telemetry = new StubTelemetry();
                ((ITelemetryInitializer)module).Initialize(telemetry);

                Assert.Null(telemetry.Context.Session.Id);
            }

            [TestMethod]
            public void InitializePreservesExistingSessionIdOfGivenTelemetry()
            {
                SessionTelemetryModule module = this.CreateSessionTelemetryModule();
                
                string expectedSessionId = "Telemetry Session ID";
                var telemetry = new StubTelemetry();
                telemetry.Context.Session.Id = expectedSessionId;
                ((ITelemetryInitializer)module).Initialize(telemetry);

                Assert.Equal(expectedSessionId, telemetry.Context.Session.Id);
            }

            [TestMethod]
            public void InitializeDoesNotSetSessionIsFirstWhenPreviousSessionTimedOut()
            {
                SessionTelemetryModule module = this.CreateSessionTelemetryModule();
                
                // Application started and suspended 1 day ago
                this.clock.Time = DateTimeOffset.Now - TimeSpan.FromDays(1);
                module.Initialize();
                module.HandleApplicationStoppingEvent(null, null);

                // Application resumes today
                this.clock.Time = DateTimeOffset.Now;
                module.HandleApplicationStartedEvent(null, null);

                var telemetry = new StubTelemetry();
                ((ITelemetryInitializer)module).Initialize(telemetry);

                Assert.Null(telemetry.Context.Session.IsFirst);
            }

            [TestMethod]
            public void InitializePreservesExistingIsFirstSessionOfGivenTelemetry()
            {
                SessionTelemetryModule module = this.CreateSessionTelemetryModule();
                module.Initialize();

                var telemetry = new StubTelemetry();
                telemetry.Context.Session.IsFirst = false;
                ((ITelemetryInitializer)module).Initialize(telemetry);

                Assert.Equal(false, telemetry.Context.Session.IsFirst);
            }
        }

        [TestClass]
        public class InitializeTelemetryConfiguration : SessionTelemetryModuleTest
        {
            [TestMethod]
            public void AddsItselfToTelemetryInitializersToSetSessionIdForAllTelemetryTypes()
            {
                ServiceLocator.AddService<IApplicationService>(new ApplicationService());
                var module = new SessionTelemetryModule(new StubPlatform(), new StubClock());
                var configuration = new TelemetryConfiguration();
                configuration.TelemetryChannel = new InMemoryChannel();
                module.Initialize();

                Assert.Contains(module, TelemetryConfiguration.Active.TelemetryInitializers);
            }
        }

        [TestClass]
        public class HandleApplicationStartedEvent : SessionTelemetryModuleTest
        {
            [TestMethod]
            public void DoesNotTrackEndIfPreviousSessionIfItIsStillActive()
            {
                ServiceLocator.AddService<IApplicationService>(new ApplicationService());
                SessionTelemetryModule module = CreateSessionTelemetryModule();

                // Application started and suspended 5 seconds ago
                this.clock.Time = DateTimeOffset.Now - TimeSpan.FromSeconds(5);
                DateTimeOffset expectedStartTime = this.clock.Time;
                module.Initialize();
                module.HandleApplicationStoppingEvent(null, null);

                // Application is resuming now
                this.clock.Time = DateTimeOffset.Now;
                module.HandleApplicationStartedEvent(null, null);

                var sessionStart = Assert.IsType<SessionStateTelemetry>(this.sentTelemetry.Single());
                Assert.Equal(expectedStartTime, sessionStart.Timestamp);
            }
        }
    }
}
