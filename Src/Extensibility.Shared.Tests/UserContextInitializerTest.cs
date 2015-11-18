namespace Microsoft.HockeyApp.Extensibility.Windows
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;
    using Channel;
    using Extensibility.Implementation;
    using Extensibility.Implementation.Platform;
    using TestFramework;
    using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
    using Assert = Xunit.Assert;
#if WINRT
    using TaskEx = System.Threading.Tasks.Task;
#endif

    [TestClass]
    public class UserContextInitializerTest
    {
        private IDictionary<string, object> applicationSettings;
        private StubPlatform platform;

        [TestInitialize]
        public void TestInitialize()
        {
            this.applicationSettings = new Dictionary<string, object>(); 
            this.platform = new StubPlatform { OnGetApplicationSettings = () => this.applicationSettings, };
            PlatformSingleton.Current = this.platform;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            PlatformSingleton.Current = null;
        }

        [Ignore]
        [TestMethod]
        public void ClassIsPublicToAllowConfigurationThroughFileAndUserCode()
        {
            Assert.True(typeof(UserContextInitializer).GetTypeInfo().IsPublic);
        }

        [TestMethod]
        public void ClassImplementsITelemetryInitializerBecauseIsFirstSessionCanChangeWhenApplicationResumes()
        {
            Assert.True(typeof(ITelemetryInitializer).IsAssignableFrom(typeof(UserContextInitializer)));
        }

        [TestMethod]
        public void InitializeSetsUserAnonymousIdOfGivenTelemetryContext()
        {
            var initializer = new UserContextInitializer();

            var telemetry = new StubTelemetry();
            initializer.Initialize(telemetry);

            Assert.NotEqual(string.Empty, telemetry.Context.User.Id);
        }

        [TestMethod]
        public void InitializeSetsSameUserAnonymousIdThroughoutLifetimeOfApplication()
        {
            var initializer = new UserContextInitializer();

            var telemetry1 = new StubTelemetry();
            initializer.Initialize(telemetry1);
            var telemetry2 = new StubTelemetry();
            initializer.Initialize(telemetry2);

            Assert.Equal(telemetry1.Context.User.Id, telemetry2.Context.User.Id);
        }

        [TestMethod]
        public void InitializeSetsSameUserAnonymousIdWhenApplicationIsRestarted()
        {
            var telemetry1 = new StubTelemetry();
            new UserContextInitializer().Initialize(telemetry1);

            var telemetry2 = new StubTelemetry();
            new UserContextInitializer().Initialize(telemetry2);

            Assert.Equal(telemetry1.Context.User.Id, telemetry2.Context.User.Id);
        }

        [TestMethod]
        public void InitializeGeneratesSingleUserIdRegardlessOfNumberOfThreadsAccessingIt()
        {
            const int NumberOfTasks = 16;
            var telemetry = new Task<ITelemetry>[NumberOfTasks];

            for (int i = 0; i < NumberOfTasks; i++)
            {               
                telemetry[i] = TaskEx.Run(() =>
                {
                    ITelemetry t = new StubTelemetry();
                    new UserContextInitializer().Initialize(t);
                    return t;
                });
            }

            Task.WaitAll(telemetry);
            string firstUserId = telemetry[0].Result.Context.User.Id;
            for (int i = 1; i < NumberOfTasks; i++)
            {
                Assert.Equal(firstUserId, telemetry[i].Result.Context.User.Id);
            }
        }

        [TestMethod]
        public void InitializerSetsUserAcquisitionDateForGivenTelemetry()
        {
            var initializer = new UserContextInitializer();

            var telemetry = new StubTelemetry();
            initializer.Initialize(telemetry);

            Assert.NotNull(telemetry.Context.User.AcquisitionDate);
        }

        [TestMethod]
        public void InitializeSetsSameUserAcquisitionDateThroughtoutLifetimeOfApplication()
        {
            var initializer = new UserContextInitializer();

            var telemetry1 = new StubTelemetry();
            initializer.Initialize(telemetry1);
            var telemetry2 = new StubTelemetry();
            initializer.Initialize(telemetry2);

            Assert.Equal(telemetry1.Context.User.AcquisitionDate, telemetry2.Context.User.AcquisitionDate);
        }

        [TestMethod]
        public void InitializeSetsSameUserAcquisitionDateWhenApplicationIsRestarted()
        {
            var telemetry1 = new StubTelemetry();
            new UserContextInitializer().Initialize(telemetry1);

            var telemetry2 = new StubTelemetry();
            new UserContextInitializer().Initialize(telemetry2);

            Assert.Equal(telemetry1.Context.User.AcquisitionDate, telemetry2.Context.User.AcquisitionDate);
        }
    }
}
