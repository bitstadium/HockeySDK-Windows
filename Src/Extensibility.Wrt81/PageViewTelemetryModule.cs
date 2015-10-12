namespace Microsoft.HockeyApp.Extensibility.Windows
{
    using System;
    using System.Threading.Tasks;
    using global::Windows.UI.Core;
    using global::Windows.UI.Xaml;

    /// <summary>
    /// Windows RunTime page view telemetry module.
    /// </summary>
    public sealed partial class PageViewTelemetryModule
    {
        /// <summary>
        /// Initialize method is called after all configuration properties have been loaded from the configuration.
        /// </summary>
        public void Initialize(TelemetryConfiguration configuration)
        {
            if (configuration != null)
            {
                this.configuration = configuration;
            }

            this.Initialization = this.InitializeAsync();
        }
        
        internal async Task InitializeAsync(CoreDispatcher dispatcher = null)
        {
            DependencyObject rootObject = null;
            while (this.currentHookRetryCount > 0)
            {
                await PlatformDispatcher.RunAsync(() => rootObject = this.GetRootObject(), dispatcher);
                
                if (rootObject != null)
                {
                    await PlatformDispatcher.RunAsync(() => this.TrackFramesInObjectTree(rootObject), dispatcher);
                    break;
                }

                this.currentHookRetryCount--;
                await Task.Delay(HookNavigationEventsRetryIntervalInMilliseconds).ConfigureAwait(false);
            }
        }

        private DependencyObject GetRootObject()
        {
            if (Window.Current != null)
            {
                return Window.Current.Content;
            }

            return null;
        }
    }
}