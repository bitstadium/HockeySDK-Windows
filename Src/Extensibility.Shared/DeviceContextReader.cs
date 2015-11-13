namespace Microsoft.HockeyApp.Extensibility
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;
    using Implementation.Tracing;

    using global::Windows.ApplicationModel.Core;
    using global::Windows.Devices.Enumeration.Pnp;
    using global::Windows.Graphics.Display;
    using global::Windows.Networking.Connectivity;
    using global::Windows.Security.Cryptography;
    using global::Windows.Security.Cryptography.Core;
    using global::Windows.Storage.Streams;
    using global::Windows.System.Profile;

    /// <summary>
    /// The reader is platform specific and will contain different implementations for reading specific data based on the platform its running on.
    /// </summary>
    internal class DeviceContextReader : IDeviceContextReader
    {
        private const string ModelNameKey = "System.Devices.ModelName";
        private const string ManufacturerKey = "System.Devices.Manufacturer";
        private const string DisplayPrimaryCategoryKey = "{78C34FC8-104A-4ACA-9EA4-524D52996E57},97";
        private const string DeviceDriverKey = "{A8B865DD-2E3D-4094-AD97-E593A70C75D6}";
        private const string DeviceDriverVersionKey = DeviceDriverKey + ",3";
        private const string DeviceDriverProviderKey = DeviceDriverKey + ",9";
        private const string RootContainer = "{00000000-0000-0000-FFFF-FFFFFFFFFFFF}";
        private const string RootContainerQuery = "System.Devices.ContainerId:=\"" + RootContainer + "\"";

        /// <summary>
        /// The singleton instance for our reader.
        /// </summary>
        private static IDeviceContextReader instance;

        /// <summary>
        /// The number of milliseconds to wait before asynchronously retrying an operation.
        /// </summary>
        private const int AsyncRetryIntervalInMilliseconds = 100;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceContextReader"/> class.
        /// </summary>
        internal DeviceContextReader()
        {
        }

        /// <summary>
        /// Initializes the current instance with respect to the platform specific implementation.
        /// </summary>
        public virtual void Initialize()
        {
        }

        /// <summary>
        /// Gets the type of the device.
        /// </summary>
        /// <returns>The type for this device as a hard-coded string.</returns>
        public virtual string GetDeviceType()
        {
            // The RawPixelsPerViewPixel property only exists on phone devices
            PropertyInfo propertyInfo = Implementation.TypeExtensions.GetProperties(typeof(DisplayInformation)).FirstOrDefault(item => string.Compare(item.Name, "RawPixelsPerViewPixel", StringComparison.Ordinal) == 0);
            return propertyInfo != null ? "Phone" : "Other";
        }

        /// <summary>
        /// Gets the device unique identifier.
        /// </summary>
        /// <returns>The discovered device identifier.</returns>
        public virtual string GetDeviceUniqueId()
        {
            string deviceId = null;
            try
            {
                // Per documentation here http://msdn.microsoft.com/en-us/library/windows/apps/jj553431.aspx we are selectively pulling out 
                // specific items from the hardware ID.
                StringBuilder builder = new StringBuilder();
                HardwareToken token = HardwareIdentification.GetPackageSpecificToken(null);
                using (DataReader dataReader = DataReader.FromBuffer(token.Id))
                {
                    int offset = 0;
                    while (offset < token.Id.Length)
                    {
                        // The first two bytes contain the type of the component and the next two bytes contain the value.
                        byte[] hardwareEntry = new byte[4];
                        dataReader.ReadBytes(hardwareEntry);

                        if ((hardwareEntry[0] == 1 || // CPU ID of the processor
                             hardwareEntry[0] == 2 || // Size of the memory
                             hardwareEntry[0] == 3 || // Serial number of the disk device
                             hardwareEntry[0] == 7 || // Mobile broadband ID
                             hardwareEntry[0] == 9) && // BIOS
                            hardwareEntry[1] == 0)
                        {
                            if (builder.Length > 0)
                            {
                                builder.Append(',');
                            }

                            builder.Append(hardwareEntry[2]);
                            builder.Append('_');
                            builder.Append(hardwareEntry[3]);
                        }

                        offset += 4;
                    }
                }

                // create a buffer containing the cleartext device ID
                IBuffer clearBuffer = CryptographicBuffer.ConvertStringToBinary(
                    builder.ToString(),
                    BinaryStringEncoding.Utf8);

                // get a provider for the SHA256 algorithm
                HashAlgorithmProvider hashAlgorithmProvider = HashAlgorithmProvider.OpenAlgorithm("SHA256");

                // hash the input buffer
                IBuffer hashedBuffer = hashAlgorithmProvider.HashData(clearBuffer);

                deviceId = CryptographicBuffer.EncodeToBase64String(hashedBuffer);
            }
            catch (Exception)
            {
                // For IoT sceanrios we will alwasy set the device id to IoT
                // Becuase HardwareIdentification API will always throw
                deviceId = "IoT";
            }

            return deviceId;
        }

        /// <summary>
        /// Gets the operating system version.
        /// </summary>
        /// <returns>The discovered operating system.</returns>
        public virtual async Task<string> GetOperatingSystemVersionAsync()
        {
            string[] requestedProperties = new string[]
                                    {
                                                   DeviceDriverVersionKey,
                                                   DeviceDriverProviderKey
                                    };

            PnpObjectCollection pnpObjects = await PnpObject.FindAllAsync(PnpObjectType.Device, requestedProperties, RootContainerQuery);

            string guessedVersion = pnpObjects.Select(item => new ProviderVersionPair
            {
                Provider = (string)GetValueOrDefault(item.Properties, DeviceDriverProviderKey),
                Version = (string)GetValueOrDefault(item.Properties, DeviceDriverVersionKey)
            })
                                              .Where(item => string.IsNullOrEmpty(item.Version) == false)
                                              .Where(item => string.Compare(item.Provider, "Microsoft", StringComparison.Ordinal) == 0)
                                              .GroupBy(item => item.Version)
                                              .OrderByDescending(item => item.Count())
                                              .Select(item => item.Key)
                                              .First();

            return guessedVersion;
        }

        /// <summary>
        /// Get the name of the manufacturer of this computer.
        /// </summary>
        /// <example>Microsoft Corporation</example>
        /// <returns>The name of the manufacturer of this computer.</returns>
        public async Task<string> GetOemName()
        {
            var rootContainer = await PnpObject.CreateFromIdAsync(PnpObjectType.DeviceContainer, RootContainer, new[] { ManufacturerKey });
            return (string)rootContainer.Properties[ManufacturerKey];
        }

        /// <summary>
        /// Get the name of the model of this computer.
        /// </summary>
        /// <example>Surface with Windows 8</example>
        /// <returns>The name of the model of this computer.</returns>
        public async Task<string> GetDeviceModel()
        {
            var rootContainer = await PnpObject.CreateFromIdAsync(PnpObjectType.DeviceContainer, RootContainer, new[] { ModelNameKey });
            return (string)rootContainer.Properties[ModelNameKey];
        }

        public async Task<string> GetScreenResolutionAsync()
        {
            string screenResolution = null;
            while (screenResolution == null)
            {
                await PlatformDispatcher.RunAsync(() =>
                {
                    double actualHeight = CoreApplication.MainView.CoreWindow.Bounds.Height;
                    double actualWidth = CoreApplication.MainView.CoreWindow.Bounds.Width;
                    DisplayInformation displayInformation = DisplayInformation.GetForCurrentView();
                    double resolutionScale = (double)displayInformation.ResolutionScale / 100;

                    PropertyInfo propertyInfo = Implementation.TypeExtensions.GetProperties(typeof(DisplayInformation))
                                                                       .FirstOrDefault(item => string.Compare(item.Name, "RawPixelsPerViewPixel", StringComparison.Ordinal) == 0);
                    if (propertyInfo != null)
                    {
                        resolutionScale = (double)propertyInfo.GetValue(displayInformation);
                    }

                    if (actualHeight > 0 && actualWidth > 0)
                    {
                        screenResolution = string.Format(CultureInfo.InvariantCulture, "{0}x{1}", (int)(actualWidth * resolutionScale), (int)(actualHeight * resolutionScale));
                    }
                });

                if (screenResolution == null)
                {
                    await Task.Delay(DeviceContextReader.AsyncRetryIntervalInMilliseconds);
                }
            }

            return screenResolution;
        }

        /// <summary>
        /// Gets the network type.
        /// </summary>
        /// <returns>The discovered network type.</returns>
        public virtual int GetNetworkType()
        {
            int result;
            try
            {
                ConnectionProfile profile = NetworkInformation.GetInternetConnectionProfile();
                if (profile == null || profile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.None)
                {
                    result = 0;
                }
                else
                {
                    result = (int)profile.NetworkAdapter.IanaInterfaceType;
                }
            }
            catch (Exception exception)
            {
                CoreEventSource.Log.LogVerbose("Fail reading Device network type: " + exception.ToString());
                result = 0;
            }

            return result;
        }
    
        /// <summary>
        /// Gets the host system locale.
        /// </summary>
        /// <returns>The discovered locale.</returns>
        public virtual string GetHostSystemLocale()
        {
            return CultureInfo.CurrentCulture.Name;
        }

        public string GetOperatingSystemName()
        {
            // currently SDK supports Windows only, so we are hardcoding this value.
            return "Windows";
        }

        /// <summary>
        /// Get the device category this computer belongs to.
        /// </summary>
        /// <example>Computer.Desktop, Computer.Tablet</example>
        /// <returns>The category of this device.</returns>
        public static async Task<string> GetDeviceCategoryAsync()
        {
            var rootContainer = await PnpObject.CreateFromIdAsync(PnpObjectType.DeviceContainer, RootContainer, new[] { DisplayPrimaryCategoryKey });
            return (string)rootContainer.Properties[DisplayPrimaryCategoryKey];
        }

        /// <summary>
        /// Get the processor architecture of this computer.
        /// </summary>
        /// <returns>The processor architecture of this computer.</returns>
        public static ProcessorArchitecture GetProcessorArchitecture()
        {
            try
            {
                var sysInfo = new _SYSTEM_INFO();
                GetNativeSystemInfo(ref sysInfo);

                return Enum.IsDefined(typeof(ProcessorArchitecture), sysInfo.wProcessorArchitecture)
                    ? (ProcessorArchitecture)sysInfo.wProcessorArchitecture
                    : ProcessorArchitecture.UNKNOWN;
            }
            catch
            {
            }

            return ProcessorArchitecture.UNKNOWN;
        }

        private static TValue GetValueOrDefault<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : default(TValue);
        }

        private class ProviderVersionPair
        {
            public string Provider { get; set; }

            public string Version { get; set; }
        }

        [DllImport("kernel32.dll")]
        static extern void GetNativeSystemInfo(ref _SYSTEM_INFO lpSystemInfo);

        [StructLayout(LayoutKind.Sequential)]
        struct _SYSTEM_INFO
        {
            public ushort wProcessorArchitecture;
            public ushort wReserved;
            public uint dwPageSize;
            public IntPtr lpMinimumApplicationAddress;
            public IntPtr lpMaximumApplicationAddress;
            public UIntPtr dwActiveProcessorMask;
            public uint dwNumberOfProcessors;
            public uint dwProcessorType;
            public uint dwAllocationGranularity;
            public ushort wProcessorLevel;
            public ushort wProcessorRevision;
        };
    }

    /// <summary>
    /// Processor Architecture
    /// </summary>
    public enum ProcessorArchitecture : ushort
    {
        /// <summary>
        /// INTEL
        /// </summary>
        INTEL = 0,

        /// <summary>
        /// MIPS
        /// </summary>
        MIPS = 1,

        /// <summary>
        /// ALPHA
        /// </summary>
        ALPHA = 2,

        /// <summary>
        /// PPC
        /// </summary>
        PPC = 3,

        /// <summary>
        /// SHX
        /// </summary>
        SHX = 4,

        /// <summary>
        /// ARM
        /// </summary>
        ARM = 5,

        /// <summary>
        /// IA64
        /// </summary>
        IA64 = 6,

        /// <summary>
        /// ALPHA64
        /// </summary>
        ALPHA64 = 7,

        /// <summary>
        /// MSIL
        /// </summary>
        MSIL = 8,

        /// <summary>
        /// AMD64
        /// </summary>
        AMD64 = 9,

        /// <summary>
        /// IA32 ON WIN64
        /// </summary>
        IA32_ON_WIN64 = 10,

        /// <summary>
        /// Unknown
        /// </summary>
        UNKNOWN = 0xFFFF
    }
}