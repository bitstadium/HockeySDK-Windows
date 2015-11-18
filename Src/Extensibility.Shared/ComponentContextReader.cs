namespace Microsoft.HockeyApp.Extensibility
{
    using System.Globalization;
    using System.Threading;
    using global::Windows.ApplicationModel;


    /// <summary>
    /// The reader is platform specific and will contain different implementations for reading specific data based on the platform its running on.
    /// </summary>
    internal partial class ComponentContextReader : IComponentContextReader
    {
        /// <summary>
        /// The default application version we will be returning if no application version is found.
        /// </summary>
        internal const string UnknownComponentVersion = "Unknown";
        
        /// <summary>
        /// The singleton instance for our reader.
        /// </summary>
        private static IComponentContextReader instance;

        /// <summary>
        /// The version for this component.
        /// </summary>
        private string version;

        private string fullPackageName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentContextReader"/> class.
        /// </summary>
        internal ComponentContextReader()
        {
        }

        /// <summary>
        /// Gets or sets the singleton instance for our application context reader.
        /// </summary>
        public static IComponentContextReader Instance
        {
            get
            {
                if (ComponentContextReader.instance != null)
                {
                    return ComponentContextReader.instance;
                }

                Interlocked.CompareExchange(ref ComponentContextReader.instance, new ComponentContextReader(), null);
                ComponentContextReader.instance.Initialize();
                return ComponentContextReader.instance;
            }

            // allow for the replacement for the context reader to allow for testability
            internal set
            {
                ComponentContextReader.instance = value;
            }
        }

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
            var currentPackage = Package.Current;
            if (currentPackage != null && currentPackage.Id != null)
            {
                temp = string.Format(
                                    CultureInfo.InvariantCulture,
                                    "{0}.{1}.{2}.{3}",
                                    currentPackage.Id.Version.Major,
                                    currentPackage.Id.Version.Minor,
                                    currentPackage.Id.Version.Build,
                                    currentPackage.Id.Version.Revision);
            }

            if (string.IsNullOrEmpty(temp) == false)
            {
                return this.version = temp;
            }

            return this.version = ComponentContextReader.UnknownComponentVersion;
        }

        /// <summary>
        /// Gets the full name of the package.
        /// </summary>
        /// <returns></returns>
        public string GetApplicationId()
        {
            if (this.fullPackageName == null)
            {
                this.fullPackageName = global::Windows.UI.Xaml.Application.Current.GetType().Namespace;
            }

            return this.fullPackageName;
        }
    }
}