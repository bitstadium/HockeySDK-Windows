namespace Microsoft.HockeyApp.Extensibility
{
    using System.Globalization;

    using Windows.ApplicationModel;

    /// <summary>
    /// The reader is platform specific and applies to Windows Phone WinRT applications only.
    /// </summary>
    internal partial class ComponentContextReader
    {
        /// <summary>
        /// The default application version we will be returning if no application version is found.
        /// </summary>
        internal const string UnknownComponentVersion = "Unknown";

        /// <summary>
        /// The version for this component.
        /// </summary>
        private string version;

        /// <summary>
        /// Initializes the current instance with respect to the platform specific implementation.
        /// </summary>
        public void Initialize()
        {
            // we don't need to do anything here.
        }

        /// <summary>
        /// Gets the version for the current application. If the version cannot be found, we will return the passed in default.
        /// </summary>
        /// <returns>The extracted data.</returns>
        public string GetVersion()
        {
            if (this.version != null)
            {
                return this.version;
            }

            string temp = null;
            if (Package.Current != null && Package.Current.Id != null)
            {
                temp = string.Format(
                                    CultureInfo.InvariantCulture,
                                    "{0}.{1}.{2}.{3}",
                                    Package.Current.Id.Version.Major,
                                    Package.Current.Id.Version.Minor,
                                    Package.Current.Id.Version.Build,
                                    Package.Current.Id.Version.Revision);
            }

            if (string.IsNullOrEmpty(temp) == false)
            {
                return this.version = temp;
            }

            return this.version = ComponentContextReader.UnknownComponentVersion;
        }
    }
}