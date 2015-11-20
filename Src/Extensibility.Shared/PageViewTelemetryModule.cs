namespace Microsoft.HockeyApp.Extensibility.Windows
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using DataContracts;
    using Extensibility;
    using HockeyApp;
    using Implementation.Tracing;
    using Microsoft.HockeyApp.Extensibility.Implementation.Platform;
#if SILVERLIGHT
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
#elif WINRT || WINDOWS_UWP
    using global::Windows.Foundation;
    using global::Windows.UI.Core;
    using global::Windows.UI.Xaml;
    using global::Windows.UI.Xaml.Controls;
    using global::Windows.UI.Xaml.Media;
    using global::Windows.UI.Xaml.Navigation;
#endif

    /// <summary>
    /// A module that deals in PageView events and will create PageViewTelemetry objects when triggered.
    /// </summary>
    internal sealed partial class PageViewTelemetryModule : ITelemetryModule, IDisposable
    {
        internal Task Initialization;

        private const string NavigationAddressUriScheme = "application:";
        private const int HookNavigationEventsRetryIntervalInMilliseconds = 100;

        private TelemetryConfiguration configuration = null;
        private TelemetryClient client;
        private List<FrameHandler> trackedFrames = new List<FrameHandler>();
        private int currentHookRetryCount = (int)TimeSpan.FromMinutes(1).TotalMilliseconds / PageViewTelemetryModule.HookNavigationEventsRetryIntervalInMilliseconds;

        /// <summary>
        /// Initializes a new instance of the <see cref="PageViewTelemetryModule"/> class.
        /// </summary>
        internal PageViewTelemetryModule()
        {
        }
        
        internal IList<FrameHandler> TrackedFrames
        {
            get { return this.trackedFrames; }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            List<FrameHandler> currentTrackedFrames = Interlocked.Exchange(ref this.trackedFrames, new List<FrameHandler>());
            if (currentTrackedFrames != null)
            {
                foreach (FrameHandler frameHandler in currentTrackedFrames)
                {
                    frameHandler.Dispose();
                }
            }
        }

        internal void TrackFramesInObjectTree(DependencyObject currentNode)
        {
            if (currentNode == null)
            {
                return;
            }

            Frame frame = currentNode as Frame;
            if (frame != null)
            {
                this.Track(frame);
                return;
            }

            int count = VisualTreeHelper.GetChildrenCount(currentNode);
            for (int i = 0; i < count; i++)
            {
                DependencyObject childNode = VisualTreeHelper.GetChild(currentNode, i);
                this.TrackFramesInObjectTree(childNode);
            }
        }
        
        private void TrackPageView(string pageName, TimeSpan duration)
        {
            TelemetryClient client = LazyInitializer.EnsureInitialized(ref this.client, () => new TelemetryClient(this.configuration));

            var telemetry = new PageViewTelemetry(pageName)
            {
                Duration = duration,
            };

            try
            {
                client.TrackPageView(telemetry);
            }
            catch (ObjectDisposedException)
            {
                Interlocked.CompareExchange(ref this.client, null, client);
            }
        }        

        private void Track(Frame frame)
        {
            if (!this.trackedFrames.Any(handler => handler.Frame == frame))
            {
                this.trackedFrames.Add(new FrameHandler(this, frame));
            }
        }
        
        internal class FrameHandler : IDisposable
        {
            private readonly PageViewTelemetryModule module;
            private readonly Stopwatch stopwatch;
            private Frame frame;

            public FrameHandler(PageViewTelemetryModule module, Frame frame)
            {
                this.module = module;

                this.frame = frame;
                this.frame.Navigating += this.HandleFrameNavigatingEvent;
                this.frame.Navigated += this.HandleFrameNavigatedEvent;
                this.frame.Unloaded += this.HandleFrameUnloadedEvent;

                this.stopwatch = new Stopwatch();

                Task dontWait = PlatformDispatcher.RunAsync(this.TrackCurrentFrameContent);
            }

            internal Frame Frame
            {
                get { return this.frame; }
            }

            public void Dispose()
            {
                Task dontWait = PlatformDispatcher.RunAsync(() =>
                {
                    // unhook the event handlers; this needs to happen on the UI thread.
                    Frame current = this.frame;
                    if (current != null)
                    {
                        current.Navigating -= this.HandleFrameNavigatingEvent;
                        current.Navigated -= this.HandleFrameNavigatedEvent;
                        current.Unloaded -= this.HandleFrameUnloadedEvent;
                        this.frame = null;
                    }
                });
            }

            private void HandleFrameNavigatingEvent(object sender, object e)
            {
                this.stopwatch.Start();
            }

            private void HandleFrameNavigatedEvent(object sender, object e)
            {
                this.stopwatch.Stop();
                this.TrackCurrentFrameContent();
            }

            private void TrackCurrentFrameContent()
            {
                if (this.frame != null && this.frame.Content != null)
                {
                    string pageName = PageViewTelemetryModule.NavigationAddressUriScheme + this.frame.Content.GetType().FullName;
                    Task dontWait = Task.Run(() => this.module.TrackPageView(pageName, this.stopwatch.Elapsed))
                        .ContinueWith(
                            task =>
                            {
                                string msg = string.Format(CultureInfo.InvariantCulture, "FrameHandler: Unhandled exception calling while tracking a page view. {0}", task.Exception);
                                CoreEventSource.Log.LogVerbose(msg);
                            },
                            TaskContinuationOptions.OnlyOnFaulted);
                }
            }

            private void HandleFrameUnloadedEvent(object sender, object e)
            {
                this.frame.Unloaded -= this.HandleFrameUnloadedEvent;
                this.module.trackedFrames.Remove(this);
            }
        }
    }
}
