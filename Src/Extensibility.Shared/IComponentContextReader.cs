namespace Microsoft.HockeyApp.Extensibility
{
    /// <summary>
    /// The component context reader interface used while reading component related information in a platform specific way.
    /// </summary>
    internal interface IComponentContextReader
    {
        /// <summary>
        /// Initializes the current reader with respect to its environment.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Gets the version for the current application. If the version cannot be found, we will return the passed in default.
        /// </summary>
        /// <returns>The extracted data.</returns>
        string GetVersion();
    }
}
