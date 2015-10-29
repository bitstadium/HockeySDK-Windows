namespace Microsoft.HockeyApp
{
    /// <summary>
    /// Public Interface for HockeyClient. 
    /// </summary>
    public interface IHockeyClient
    {
        /// <summary>
        /// Bootstraps HockeyApp SDK.
        /// </summary>
        /// <param name="appId">App ID.</param>
        void Configure(string appId);
    }
}
