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

      
    }
}