namespace Microsoft.HockeyApp.Services
{
    using System;
    using System.Security.Cryptography;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Phone.Info;
    using global::Windows.Networking.Connectivity;
    using System.Xml.Linq;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Xml;

    /// <summary>
    /// The reader is platform specific and applies to Windows Phone Silverlight applications only.
    /// </summary>
    internal partial class DeviceService : IDeviceService
    {
        /// <summary>
        /// The file name used when storing persistent context.
        /// </summary>
        internal const string ContextPersistentStorageFileName = "Micrsooft.HockeyApp.DeviceContext.xml";

        /// <summary>
        /// The device identifier.
        /// </summary>
        private string deviceId;

        /// <summary>
        /// The operating system.
        /// </summary>
        private string operatingSystem;

        /// <summary>
        /// The device manufacturer.
        /// </summary>
        private string deviceManufacturer;

        /// <summary>
        /// The device name.
        /// </summary>
        private string deviceName;

        /// <summary>
        /// The network type.
        /// </summary>
        private int? networkType;

        /// <summary>
        /// The cached fallback context.
        /// </summary>
        private FallbackDeviceContext cachedContext;

        private readonly object syncRoot = new object();

        /// <summary>
        /// Gets the fallback device context.
        /// </summary>
        private FallbackDeviceContext FallbackContext
        {
            get
            {
                FallbackDeviceContext output = this.ReadSerializedContext();
                return output;
            }
        }

        /// <summary>
        /// Gets the type of the device.
        /// </summary>
        /// <returns>The type for this device as a hard-coded string.</returns>
        public virtual string GetDeviceType()
        {
            return "Phone";
        }

        /// <summary>
        /// Gets the device unique ID, or uses the fallback if none is available due to application configuration.
        /// </summary>
        /// <returns>
        /// The discovered device identifier.
        /// </returns>
        public virtual string GetDeviceUniqueId()
        {
            if (this.deviceId != null)
            {
                return this.deviceId;
            }

            object uniqueId;
            if (DeviceExtendedProperties.TryGetValue("DeviceUniqueId", out uniqueId) == true)
            {
                byte[] result = (byte[])uniqueId;
                using (SHA256 hasher = new SHA256Managed())
                {
                    return this.deviceId = Convert.ToBase64String(hasher.ComputeHash(result));
                }
            }

            FallbackDeviceContext fallbackContext = this.FallbackContext;
            return this.deviceId = fallbackContext.DeviceUniqueId;
        }

        /// <summary>
        /// Gets the device OEM.
        /// </summary>
        /// <returns>The discovered OEM.</returns>
        public Task<string> GetOemName()
        {
            if (deviceManufacturer == null)
            {
                deviceManufacturer = DeviceStatus.DeviceManufacturer;
            }

            return Task.FromResult<string>(this.deviceManufacturer);
        }

        /// <summary>
        /// Gets the device model.
        /// </summary>
        /// <returns>The discovered device model.</returns>
        public string GetDeviceModel()
        {
            if (this.deviceName != null)
            {
                return this.deviceName;
            }

            return this.deviceName = DeviceStatus.DeviceName;
        }

        /// <summary>
        /// Gets the network type.
        /// </summary>
        /// <returns>The discovered network type.</returns>
        public virtual int GetNetworkType()
        {
            if (this.networkType.HasValue == true)
            {
                return this.networkType.Value;
            }

            ConnectionProfile profile = NetworkInformation.GetInternetConnectionProfile();
            try
            {
                if (profile == null ||
                    profile.NetworkAdapter == null ||
                    profile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.None)
                {
                    this.networkType = 0;
                    return 0;
                }
            }
            catch (Exception)
            {
                // Note: GetNetworkConnectivityLevel() is not supported on WP 8.0
            }

            this.networkType = (int)profile.NetworkAdapter.IanaInterfaceType;
            return this.networkType.Value;
        }

        Task<string> IDeviceService.GetDeviceType()
        {
            return Task.FromResult<string>("Phone");
        }

        public Task<string> GetOperatingSystemVersionAsync()
        {
            if (this.operatingSystem == null)
            {
                this.operatingSystem = Environment.OSVersion.Version.ToString(4);
            }

            return Task.FromResult<string>(this.operatingSystem);
        }

        public string GetHostSystemLocale()
        {
            return "";
        }

        public string GetOperatingSystemName()
        {
            return "Windows";
        }

        /// <summary>
        /// Reads the serialized context from persistent storage, or will create a new context if none exits.
        /// </summary>
        /// <returns>The fallback context we will be using.</returns>
        private FallbackDeviceContext ReadSerializedContext()
        {
            // if we already have a context, just return that
            if (this.cachedContext != null)
            {
                return this.cachedContext;
            }

            // if we don't aquire the sync root and check again, in case we were waiting before
            lock (this.syncRoot)
            {
                if (this.cachedContext != null)
                {
                    return this.cachedContext;
                }

                FallbackDeviceContext temp = ReadSerializedContext(DeviceService.ContextPersistentStorageFileName);
                Thread.MemoryBarrier();
                this.cachedContext = temp;
            }

            return this.cachedContext;
        }

        /// <summary>
        /// Reads the serialized context from persistent storage, or will create a new context if none exits.
        /// </summary>
        /// <param name="fileName">The file to read from storage.</param>
        /// <returns>The fallback context we will be using.</returns>
        private static FallbackDeviceContext ReadSerializedContext(string fileName)
        {
            // get a reference to the persitent store
            using (IsolatedStorageFile persistentStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                // if the file exits, attempt to read/deserialize it. If we fail, we'll just regen it.
                bool regenerateContext = true;
                if (persistentStore.FileExists(fileName) == true)
                {
                    try
                    {
                        using (IsolatedStorageFileStream stream = persistentStore.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                        {
                            XDocument document = XDocument.Load(stream);
                            FallbackDeviceContext temp = new FallbackDeviceContext();
                            if (temp.Deserialize(document.Root) == true)
                            {
                                regenerateContext = false;
                                return temp;
                            }
                        }
                    }
                    catch (IsolatedStorageException)
                    {
                        // TODO: swallow?
                    }
                    catch (FileNotFoundException)
                    {
                        // TODO: swallow?
                    }
                    catch (XmlException)
                    {
                        // TODO: swallow?
                    }
                }

                // if we're here we will need to regen our context
                if (regenerateContext == true)
                {
                    // create the XML document first
                    XDocument document = new XDocument();
                    document.Add(new XElement(XName.Get(typeof(FallbackDeviceContext).Name)));

                    // initialize the new set of settings and serialize to the document root
                    FallbackDeviceContext temp = new FallbackDeviceContext();
                    temp.Initialize();
                    temp.Serialize(document.Root);

                    // write to persistent storage
                    try
                    {
                        using (IsolatedStorageFileStream stream = persistentStore.OpenFile(fileName, FileMode.OpenOrCreate, FileAccess.Write))
                        {
                            stream.SetLength(0);
                            document.Save(stream);
                            stream.Flush(true);
                            return temp;
                        }
                    }
                    catch (IsolatedStorageException)
                    {
                        // TODO: swallow?
                    }
                    catch (FileNotFoundException)
                    {
                        // TODO: swallow?
                    }
                }
            }

            FallbackDeviceContext defaultReturn = new FallbackDeviceContext();
            defaultReturn.Initialize();
            return defaultReturn;
        }


        class FallbackDeviceContext
        {
            /// <summary>
            /// Gets the device unique identifier.
            /// </summary>
            public string DeviceUniqueId { get; private set; }

            /// <summary>
            /// Initializes this instance with a set of new properties to serve as context.
            /// </summary>
            public void Initialize()
            {
                byte[] buffer = new byte[20];
                Random random = new Random();
                random.NextBytes(buffer);

                this.DeviceUniqueId = Convert.ToBase64String(buffer);
            }

            /// <summary>
            /// Serializes the current instance to the passed in root element.
            /// </summary>
            /// <param name="rootElement">The root element to serialize to.</param>
            public void Serialize(XElement rootElement)
            {
                rootElement.Add(new XElement(XName.Get("DeviceUniqueId"), this.DeviceUniqueId));
            }

            /// <summary>
            /// Deserializes the passed in root element to the current instance.
            /// </summary>
            /// <param name="rootElement">The root element to deserialize.</param>
            /// <returns>True if deserialization was successful, false otherwise.</returns>
            public bool Deserialize(XElement rootElement)
            {
                if (rootElement == null)
                {
                    return false;
                }

                XElement deviceUniqueIdElement = rootElement.Element(XName.Get("DeviceUniqueId"));
                if (deviceUniqueIdElement == null)
                {
                    return false;
                }

                this.DeviceUniqueId = deviceUniqueIdElement.Value;
                if (this.DeviceUniqueId.IsNullOrWhiteSpace() == true)
                {
                    return false;
                }

                return true;
            }
        }
    }
}
