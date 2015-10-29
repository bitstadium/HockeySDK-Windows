namespace Microsoft.HockeyApp
{
    using Windows.ApplicationModel;

    /// <summary>
    ///  Send information to the HockeyApp service.
    /// </summary>
    public class HockeyClient : IHockeyClient
    {
        /// <summary>
        /// Bootstraps HockeyApp SDK.
        /// </summary>
        /// <param name="appId">App ID.</param>
        public void Configure(string appId)
        {
             WindowsAppInitializer.InitializeAsync(appId);
        }
    }
}
