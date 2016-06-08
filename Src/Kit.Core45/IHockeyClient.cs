namespace Microsoft.HockeyApp
{
    /// <summary>
    /// Public Interface for HockeyClient. Used by static extension methods in platfomr-specific SDKs
    /// </summary>
    public interface IHockeyClient
    {
        /// <summary>
        /// Send a custom event for display in Events tab.
        /// </summary>
        /// <param name="eventName">Event name</param>
        void TrackEvent(string eventName);
    }
}
