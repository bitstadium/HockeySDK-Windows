namespace Microsoft.HockeyApp.Extensibility
{
    using global::Windows.Globalization;

    /// <summary>
    /// The reader is platform specific and applies to WinRT applications only.
    /// </summary>
    internal class UserContextReader
    {
        /// <summary>
        /// Gets the store region.
        /// </summary>
        /// <returns>The two-letter identifier for the user's region.</returns>
        public static string GetStoreRegion()
        {
            var userRegion = new GeographicRegion();
            string regionCode = userRegion.CodeTwoLetter;

            return regionCode;
        }
    }
}