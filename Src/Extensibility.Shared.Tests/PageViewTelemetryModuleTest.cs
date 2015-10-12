namespace Microsoft.ApplicationInsights.Extensibility.Windows
{
    using Channel;
    using DataContracts;
    using TestFramework;
    using System;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
#if SILVERLIGHT
    using System.Windows;
    using System.Windows.Controls;
#endif
    using Microsoft.ApplicationInsights.Windows;
    using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#if WINRT
    using global::Windows.UI.Xaml;
    using global::Windows.UI.Xaml.Controls;
#endif
    using Assert = Xunit.Assert;

    [TestClass]
    public class PageViewTelemetryModuleTest
    {
        private Frame frame;

        public PageViewTelemetryModuleTest()
        {
#if WINRT     
            PlatformDispatcher.RunAsync(() => { Window.Current.Content = this.frame = new Frame(); }).GetAwaiter().GetResult();
#else
            PlatformDispatcher.RunAsync(() => { this.frame = (Frame)Application.Current.RootVisual; }).GetAwaiter().GetResult();
#endif
        }

        protected static UIElement RootUIElement
        {
#if WINRT
            get { return Window.Current.Content; }
#else
            get { return Application.Current.RootVisual; }
#endif
        }

        private TelemetryConfiguration GetDisconnectedConfiguration(ITelemetryChannel channel = null)
        {
            return new TelemetryConfiguration
            {
                InstrumentationKey = Guid.NewGuid().ToString(),
                TelemetryChannel = channel ?? new StubTelemetryChannel()
            };
        }

        private async Task<PageViewTelemetryModule> GetCleanModule()
        {
            PageViewTelemetryModule module = new PageViewTelemetryModule();
            module.Initialize(this.GetDisconnectedConfiguration());
            await module.Initialization;

            // clear out the instantiated run (due to the automatic invocation of InitializeAsync()
            foreach (PageViewTelemetryModule.FrameHandler frameHandler in module.TrackedFrames)
            {
                frameHandler.Dispose();
            }

            module.TrackedFrames.Clear();

            return module;
        }

        [TestClass]
        public sealed class Class : PageViewTelemetryModuleTest, IDisposable
        {
            private ITelemetry sentTelemetry;
            private ManualResetEventSlim telemetrySent;
            private StubTelemetryChannel channel;

            public new void Dispose()
            {
                this.telemetrySent.Dispose();
            }

            [TestInitialize]
            public void TestInitialize()
            {
                this.telemetrySent = new ManualResetEventSlim();
                this.channel = new StubTelemetryChannel
                {
                    OnSend = telemetry =>
                    {
                        sentTelemetry = telemetry;
                        telemetrySent.Set();
                    },
                };
            }

            [TestCleanup]
            public async Task Cleanup()
            {
                await PlatformDispatcher.RunAsync(() =>
                {
                    if (this.frame.CanGoBack)
                    {
                        this.frame.GoBack();
                    }
                });
            }
            
            [TestMethod]
            public async Task GeneratesPageViewTelemetryWhenFrameIsDetected()
            {
                await this.NavigateToTestPageAsync();
                using (PageViewTelemetryModule module = new PageViewTelemetryModule())
                {
                    module.Initialize(this.GetDisconnectedConfiguration(this.channel));
                    Assert.True(this.telemetrySent.Wait(5000));
                    var telemetry = this.sentTelemetry as PageViewTelemetry;
                    Assert.Equal("application:" + typeof(TestPage).FullName, telemetry.Name);
                }
            }

            [TestMethod]
            public async Task GeneratesPageViewTelemetryWhenFrameNavigationOccurs()
            {
                using (PageViewTelemetryModule module = new PageViewTelemetryModule())
                {
                    module.Initialize(this.GetDisconnectedConfiguration(this.channel));
                    await module.Initialization;
                    await this.NavigateToTestPageAsync();

                    Assert.True(this.telemetrySent.Wait(5000));
                    var telemetry = this.sentTelemetry as PageViewTelemetry;
                    Assert.Equal("application:" + typeof(TestPage).FullName, telemetry.Name);
                    Assert.True(telemetry.Duration > TimeSpan.Zero);
                }
            }

            private Task NavigateToTestPageAsync()
            {
                var tcs = new TaskCompletionSource<object>();

                Task dontWait = PlatformDispatcher.RunAsync(() =>
                {
                    this.frame.Navigated += (s, e) => { tcs.TrySetResult(null); };
#if WINRT
                this.frame.Navigate(typeof(TestPage));
#else
                    this.frame.Navigate(new Uri("/TestPage.xaml", UriKind.RelativeOrAbsolute));
#endif
                });

                return tcs.Task;
            }
        }

        [TestClass]
        public class Dispose : PageViewTelemetryModuleTest
        {
            [TestMethod]
            public async Task CanBeCalledFromTheUIThreadAndWillDisposeOfItems()
            {
                using (TelemetryConfiguration config = this.GetDisconnectedConfiguration())
                using (PageViewTelemetryModule module = new PageViewTelemetryModule())
                {
                    module.Initialize(config);
                    await module.Initialization;
                    await PlatformDispatcher.RunAsync(() =>
                    {
                        module.Dispose();
                        Assert.Equal(0, module.TrackedFrames.Count);
                    });
                }
            }

            [TestMethod]
            public async Task CanBeCalledFromAThreadDifferentThanTheUIThreadAndWillDisposeOfItems()
            {
                using (TelemetryConfiguration config = this.GetDisconnectedConfiguration())
                using (PageViewTelemetryModule module = new PageViewTelemetryModule())
                {
                    module.Initialize(config);
                    await module.Initialization;
                    await Task.Factory.StartNew(() =>
                    {
                        module.Dispose();
                        Assert.Equal(0, module.TrackedFrames.Count);
                    });
                }
            }
        }

        [TestClass]
        public class TrackFramesInObjectTree : PageViewTelemetryModuleTest
        {
            [TestMethod]
            public async Task FindsFrameInObjectTree()
            {
                using (PageViewTelemetryModule module = await this.GetCleanModule())
                {
                    await PlatformDispatcher.RunAsync(() =>
                    {
                        module.TrackFramesInObjectTree(RootUIElement);
                        Assert.Equal(1, module.TrackedFrames.Count);
                        Assert.Same(RootUIElement, module.TrackedFrames[0].Frame);
                    });
                }
            }

            [TestMethod]
            public async Task DoesNotProcessAnythingOtherThanFramesAndNoFrameIsProcessedMoreThanOnce()
            {
                using (PageViewTelemetryModule module = await this.GetCleanModule())
                {
                    await PlatformDispatcher.RunAsync(() =>
                    {
                        module.TrackFramesInObjectTree(new Grid());
                        Assert.Equal(0, module.TrackedFrames.Count);

                        module.TrackFramesInObjectTree(RootUIElement);
                        Assert.Equal(1, module.TrackedFrames.Count);
                        Assert.Same(RootUIElement, module.TrackedFrames[0].Frame);

                        module.TrackFramesInObjectTree(RootUIElement);
                        Assert.Equal(1, module.TrackedFrames.Count);
                        Assert.Same(RootUIElement, module.TrackedFrames[0].Frame);
                    });
                }
            }
        }
    }
}