namespace Microsoft.HockeyApp.Extensibility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Runtime.InteropServices;

    using global::Windows.Devices.Enumeration.Pnp;

    internal static class PlatformHacks
    {
        private const string ModelNameKey = "System.Devices.ModelName";
        private const string ManufacturerKey = "System.Devices.Manufacturer";
        private const string DisplayPrimaryCategoryKey = "{78C34FC8-104A-4ACA-9EA4-524D52996E57},97";
        private const string DeviceDriverKey = "{A8B865DD-2E3D-4094-AD97-E593A70C75D6}";
        private const string DeviceDriverVersionKey = PlatformHacks.DeviceDriverKey + ",3";
        private const string DeviceDriverProviderKey = PlatformHacks.DeviceDriverKey + ",9";
        private const string RootContainer = "{00000000-0000-0000-FFFF-FFFFFFFFFFFF}";
        private const string RootContainerQuery = "System.Devices.ContainerId:=\"" + PlatformHacks.RootContainer + "\"";

        public static async Task<string> GetWindowsVersion()
        {
            string[] requestedProperties = new string[]
                                               {
                                                   PlatformHacks.DeviceDriverVersionKey, 
                                                   PlatformHacks.DeviceDriverProviderKey
                                               };

            PnpObjectCollection pnpObjects = await PnpObject.FindAllAsync(PnpObjectType.Device, requestedProperties, PlatformHacks.RootContainerQuery);

            string guessedVersion = pnpObjects.Select(item => new ProviderVersionPair
                                                                  {
                                                                      Provider = (string)GetValueOrDefault(item.Properties, PlatformHacks.DeviceDriverProviderKey),
                                                                      Version = (string)GetValueOrDefault(item.Properties, PlatformHacks.DeviceDriverVersionKey)
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
        /// Get the name of the model of this computer.
        /// </summary>
        /// <example>Surface with Windows 8</example>
        /// <returns>The name of the model of this computer.</returns>
        public static async Task<string> GetDeviceModelAsync()
        {
            var rootContainer = await PnpObject.CreateFromIdAsync(PnpObjectType.DeviceContainer, RootContainer, new[] { ModelNameKey });
            return (string)rootContainer.Properties[ModelNameKey];
        }

        /// <summary>
        /// Get the name of the manufacturer of this computer.
        /// </summary>
        /// <example>Microsoft Corporation</example>
        /// <returns>The name of the manufacturer of this computer.</returns>
        public static async Task<string> GetDeviceManufacturerAsync()
        {
            var rootContainer = await PnpObject.CreateFromIdAsync(PnpObjectType.DeviceContainer, RootContainer, new[] { ManufacturerKey });
            return (string)rootContainer.Properties[ManufacturerKey];
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


        private static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key)
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