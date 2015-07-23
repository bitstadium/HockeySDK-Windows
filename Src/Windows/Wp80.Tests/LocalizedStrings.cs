namespace Microsoft.ApplicationInsights.Windows.Wp80.Tests
{
    using Microsoft.ApplicationInsights.Windows.Wp80.Tests.Resources;

    /// <summary>
    /// Provides access to string resources.
    /// </summary>
    public class LocalizedStrings
    {
        /// <summary>
        /// The set or localized resources.
        /// </summary>
        private static AppResources localizedResources = new AppResources();

        /// <summary>
        /// Gets the localized resources.
        /// </summary>
        public AppResources LocalizedResources
        {
            get { return localizedResources; }
        }
    }
}