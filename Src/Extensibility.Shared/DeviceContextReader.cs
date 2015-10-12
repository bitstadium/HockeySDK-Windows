namespace Microsoft.HockeyApp.Extensibility
{
    using System.Globalization;
    using System.Threading;

    /// <summary>
    /// The reader is platform specific and will contain different implementations for reading specific data based on the platform its running on.
    /// </summary>
    internal partial class DeviceContextReader : 
        IDeviceContextReader
    {
        /// <summary>
        /// The file name used when storing persistent context.
        /// </summary>
        internal const string ContextPersistentStorageFileName = "ApplicationInsights.DeviceContext.xml";

        /// <summary>
        /// The singleton instance for our reader.
        /// </summary>
        private static IDeviceContextReader instance;

        /// <summary>
        /// The sync root used in synchronizing access to persistent storage.
        /// </summary>
        private readonly object syncRoot = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceContextReader"/> class.
        /// </summary>
        internal DeviceContextReader()
        {
        }

        /// <summary>
        /// Gets or sets the singleton instance for our application context reader.
        /// </summary>
        public static IDeviceContextReader Instance
        {
            get
            {
                if (DeviceContextReader.instance != null)
                {
                    return DeviceContextReader.instance;
                }

                Interlocked.CompareExchange(ref DeviceContextReader.instance, new DeviceContextReader(), null);
                DeviceContextReader.instance.Initialize();
                return DeviceContextReader.instance;
            }

            // allow for the replacement for the context reader to allow for testability
            internal set
            {
                DeviceContextReader.instance = value;
            }
        }

        /// <summary>
        /// Gets the host system locale.
        /// </summary>
        /// <returns>The discovered locale.</returns>
        public virtual string GetHostSystemLocale()
        {
            return CultureInfo.CurrentCulture.Name;
        }
    }
}