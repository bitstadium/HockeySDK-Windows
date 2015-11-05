namespace Microsoft.HockeyApp
{
    using System;

    /// <summary>
    /// Send information to the HockeyApp service.
    /// </summary>
    public class HockeyClient : IHockeyClient
    {
        private static readonly Lazy<HockeyClient> lazy = new Lazy<HockeyClient>(() => new HockeyClient());

        /// <summary>
        /// Gets the current singleton instance of HockeyClient.
        /// </summary>
        public static IHockeyClient Current
        {
            get { return lazy.Value; }
        }

        /// <summary>
        /// Bootstraps HockeyApp SDK.
        /// </summary>
        /// <param name="appId">App ID.</param>
        public void Configure(string appId)
        {
            // Initializing collectors with WindowsCollectors.UnhandledException for UWP - in UWP we will use Application Insights pipeline for crash collection.
            WindowsAppInitializer.InitializeAsync(appId, WindowsCollectors.Metadata | WindowsCollectors.Session | WindowsCollectors.PageView | WindowsCollectors.UnhandledException);
        }
    }
}
