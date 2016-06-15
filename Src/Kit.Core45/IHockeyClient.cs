using System.Collections.Generic;

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
        /// <param name="eventName">A name for the event.</param>
        /// <param name="properties">Named string values you can use to search and classify events.</param>
        /// <param name="metrics">Measurements associated with this event.</param>
        void TrackEvent(
            string eventName,
            IDictionary<string, string> properties = null,
            IDictionary<string, double> metrics = null);
    }
}
