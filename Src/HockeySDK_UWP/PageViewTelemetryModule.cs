namespace Microsoft.HockeyApp.Extensibility.Windows
{
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

            // ToDo: Implement Initialize method
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