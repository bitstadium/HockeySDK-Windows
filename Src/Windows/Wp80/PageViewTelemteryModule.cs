namespace Microsoft.ApplicationInsights.Extensibility.Windows
{
    using System;
    using System.Threading.Tasks;
    using System.Windows;

    /// <summary>
    /// PageView telemetry module for WP80.
    /// </summary>
    public sealed partial class PageViewTelemetryModule
    {
        /// <summary>
        /// Initializing the PageView telemetry module for WP80.
        /// </summary>
        /// <param name="configuration">Telemetry configuration.</param>
        public void Initialize(TelemetryConfiguration configuration)
        {
            if (configuration != null)
            {
                this.configuration = configuration;
            }

            this.Initialization = this.InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            DependencyObject rootObject = null;
            while (this.currentHookRetryCount > 0)
            {
                await PlatformDispatcher.RunAsync(() => rootObject = this.GetRootObject());

                if (rootObject != null)
                {
                    await PlatformDispatcher.RunAsync(() => this.TrackFramesInObjectTree(rootObject));
                    break;
                }

                this.currentHookRetryCount--;
                await Task.Delay(HookNavigationEventsRetryIntervalInMilliseconds).ConfigureAwait(false);
            }
        }

        private DependencyObject GetRootObject()
        {
            if (Application.Current != null)
            {
                return Application.Current.RootVisual;
            }

            return null;
        }
    }
}
