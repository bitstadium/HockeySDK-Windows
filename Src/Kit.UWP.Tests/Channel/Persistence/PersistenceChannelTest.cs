namespace Microsoft.HockeyApp.Channel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Extensibility.Implementation.Platform;
    using TestFramework;
    using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
    using TaskEx = System.Threading.Tasks.Task;
    using Services;

    public class PersistenceChannelTest : AsyncTest
    {
        private readonly TelemetryConfiguration configuration;

        public PersistenceChannelTest()
        {
            this.configuration = new TelemetryConfiguration();            
        }

        protected override void Dispose(bool disposing)
        {
            this.configuration.Dispose();            
            PlatformSingleton.Current = null;
        }

        [TestClass]
        public class DeveloperMode : PersistenceChannelTest
        {
            [TestMethod]
            public void DeveloperModeIsFalseByDefault()
            {
                ServiceLocator.AddService<BaseStorageService>(new StorageService());
                var channel = new PersistenceChannel();
                Assert.IsFalse(channel.DeveloperMode.Value);
            }

            [TestMethod]
            public void DeveloperModeCanBeModifiedByConfiguration()
            {
                ServiceLocator.AddService<BaseStorageService>(new StorageService());
                var channel = new PersistenceChannel();
                channel.DeveloperMode = true;
                Assert.IsTrue(channel.DeveloperMode.Value);
            }

            [TestMethod]
            public void WhenSetToTrueChangesTelemetryBufferCapacityToOneForImmediateTransmission()
            {
                ServiceLocator.AddService<BaseStorageService>(new StorageService());
                var channel = new PersistenceChannel();
                channel.DeveloperMode = true;
                Assert.AreEqual(1, channel.TelemetryBuffer.Capacity);
            }

            [TestMethod]
            public void WhenSetToFalseChangesTelemetryBufferCapacityToOriginalValueForBufferedTransmission()
            {
                ServiceLocator.AddService<BaseStorageService>(new StorageService());
                var channel = new PersistenceChannel();
                int originalTelemetryBufferSize = channel.TelemetryBuffer.Capacity;

                channel.DeveloperMode = true;
                channel.DeveloperMode = false;

                Assert.AreEqual(originalTelemetryBufferSize, channel.TelemetryBuffer.Capacity);
            }
        }

        [TestClass]
        public class EndpointAddress : PersistenceChannelTest
        {
            [TestMethod]
            public void EndpointAddressCanBeModifiedByConfiguration()
            {
                ServiceLocator.AddService<BaseStorageService>(new StorageService());
                var channel = new PersistenceChannel();
               
                Uri expectedEndpoint = new Uri("http://abc.com");
                channel.EndpointAddress = expectedEndpoint.AbsoluteUri;

                Assert.AreEqual(expectedEndpoint, new Uri(channel.EndpointAddress));
            }
        }

        [TestClass]
        public class MaxTelemetryBufferCapacity : PersistenceChannelTest
        {
            [TestMethod]
            public void GetterReturnsTelemetryBufferCapacity()
            {
                ServiceLocator.AddService<BaseStorageService>(new StorageService());
                var channel = new PersistenceChannel();
                channel.TelemetryBuffer.Capacity = 42;
                Assert.AreEqual(42, channel.MaxTelemetryBufferCapacity);
            }

            [TestMethod]
            public void SetterChangesTelemetryBufferCapacity()
            {
                ServiceLocator.AddService<BaseStorageService>(new StorageService());
                var channel = new PersistenceChannel();
                channel.MaxTelemetryBufferCapacity = 42;
                Assert.AreEqual(42, channel.TelemetryBuffer.Capacity);
            }
        }

        [TestClass]
        public class Send : PersistenceChannelTest
        {
            [TestMethod]
            public void PassesTelemetryToMemoryBufferChannel()
            {
                ServiceLocator.AddService<BaseStorageService>(new StorageService());
                var channel = new PersistenceChannel();

                var telemetry = new StubTelemetry();
                channel.Send(telemetry);

                IEnumerable<ITelemetry> actual = channel.TelemetryBuffer.Dequeue();
                Assert.AreEqual(telemetry, actual.First());
            }
        }

        [TestClass]
        public class ConstructorTest : PersistenceChannelTest
        {
            /// <summary>
            /// Testing that creating multiple instances of PersistenceChannel and Disposing them is not causing a deadlock.
            /// </summary>
            [Owner("mihailsm")]
            [TestMethod]
            public void TestPersistenceChannelConstructorAndDisposeOnDeadlock()
            {
                ServiceLocator.AddService<BaseStorageService>(new StorageService());
                List<Task> taskList = new List<Task>();
                for (int i = 0; i < 500; i++)
                {
                    var task = new Task(Action);
                    task.Start();
                    taskList.Add(task);
                }

                var completed = Task.WaitAll(taskList.ToArray(), 5000);
                Assert.IsTrue(completed, "tasks did not finish. Potential deadlock problem.");
            }

            private void Action()
            {
                var channel = new PersistenceChannel();
                channel.Dispose();
            }
        }
    }
}
