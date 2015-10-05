namespace Microsoft.ApplicationInsights.Extensibility
{
    /// <summary>
    /// The reader is platform specific and applies to Silverlight applications only.
    /// </summary>
    public class UserContextReader
    {
        /// <summary>
        /// Gets the store region.
        /// </summary>
        /// <returns>Since there is no API for this Silverlight-based apps always returns null.</returns>
        public static string GetStoreRegion()
        {
            return null;
        }
    }
}