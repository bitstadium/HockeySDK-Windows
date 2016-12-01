using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Security.Cryptography;
using System.Net.NetworkInformation;
using Microsoft.Win32;

namespace Microsoft.HockeyApp.Services.Device
{
    /// <summary>
    /// The device service
    /// </summary>
    internal sealed class DeviceService : IDeviceService
    {
        /// <summary>
        /// The device unique Id
        /// </summary>
        private string deviceUniqueId = null;

        /// <summary>
        /// The management access service
        /// </summary>
        private readonly WmiService wmiService = new WmiService();

        /// <summary>
        /// Gets the value without throwing an exception.
        /// </summary>
        /// <typeparam name="T">The generic type parameter</typeparam>
        /// <param name="wrapper">The wrapper.</param>
        /// <returns>The value</returns>
        private T GetValue<T>(Func<T> wrapper)
        {
            try
            {
                return wrapper();
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// Gets the value without throwing an exception.
        /// </summary>
        /// <typeparam name="T">The generic type parameter</typeparam>
        /// <param name="wrapper">The wrapper.</param>
        /// <param name="default">The default value in case an exception occurs</param>
        /// <returns>The value</returns>
        private T GetValue<T>(Func<T> wrapper, T @default)
        {
            try
            {
                return wrapper();
            }
            catch
            {
                return @default;
            }
        }

        /// <summary>
        /// Gets the system manufacturer.
        /// </summary>
        /// <returns></returns>
        public string GetSystemManufacturer()
        {
            return GetValue(() => wmiService.GetManagementProperty<string>("Win32_ComputerSystem", "Manufacturer"), "Unknown");
        }

        /// <summary>
        /// Gets the type of the device.
        /// </summary>
        /// <returns>
        /// The type for this device as a hard-coded string.
        /// </returns>
        public async Task<string> GetDeviceType()
        {
            return await Task.Run<string>(() => GetValue(() => ((ChassisType)wmiService.GetManagementProperty<ushort[]>("Win32_SystemEnclosure", "chassistypes")[0]), ChassisType.Unknown).ToString());
        }

        /// <summary>
        /// Gets the device unique identifier.
        /// </summary>
        /// <returns>
        /// The discovered device identifier.
        /// </returns>
        public string GetDeviceUniqueId()
        {
            if (deviceUniqueId == null)
            {
                try
                {
                    var deviceIdentifier = string.Join(",",
                        GetValue(() => string.Join(",", wmiService.GetManagementProperties<string>("Win32_Processor", "ProcessorID"))),
                        GetValue(() => string.Join(",", wmiService.GetManagementProperties<string>("Win32_LogicalDisk", "VolumeSerialNumber"))),
                        GetValue(() => string.Join(",", wmiService.GetManagementProperties<ulong>("Win32_ComputerSystem", "TotalPhysicalMemory").Select(x => x.ToString(CultureInfo.InvariantCulture)))),
                        GetValue(() => wmiService.GetManagementProperty<string>("Win32_BIOS", "Manufacturer")),
                        GetValue(() => wmiService.GetManagementProperty<string>("Win32_BIOS", "Version")),
                        GetValue(() => wmiService.GetManagementProperty<string>("Win32_BIOS", "SerialNumber")),
                        GetValue(() => GetActiveNetworkInterface().GetPhysicalAddress().ToString(), string.Empty)
                    );

                    var hash = SHA256Managed.Create().ComputeHash(Encoding.UTF8.GetBytes(deviceIdentifier));
                    deviceUniqueId = Convert.ToBase64String(hash);
                }
                catch (Exception)
                {
                    // This is not reliable, try clause shouldn't throw
                    deviceUniqueId = "IoT";
                }
            }

            return deviceUniqueId;
        }

        /// <summary>
        /// Gets the operating system version.
        /// </summary>
        /// <returns>
        /// The discovered operating system.
        /// </returns>
        public async Task<string> GetOperatingSystemVersionAsync()
        {
            // Inner method call won't throw ever
            return await Task.Run<string>(() => GetOperatingSystemVersion());
        }

        /// <summary>
        /// Gets the operating system version.
        /// </summary>
        /// <returns></returns>
        public string GetOperatingSystemVersion()
        {
            return GetValue(GetOperatingSystemVersionFromRegistry, GetValue(GetOperationSystemVersionFromEnvironment));
        }

        /// <summary>
        /// Gets the registry operating system version.
        /// </summary>
        /// <returns>The operating system version retrieved by Environment</returns>
        /// <remarks>
        /// Created for readability purposes only.
        /// </remarks>
        private string GetOperatingSystemVersionFromRegistry()
        {
            using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion"))
            {
                return (string)registryKey.GetValue("CurrentVersion") + "." + (string)registryKey.GetValue("CurrentBuild") + ".0";
            }
        }

        /// <summary>
        /// Gets the environment operation system version.
        /// </summary>
        /// <returns>The operating system version retrieved by Environment</returns>
        /// <remarks>
        /// Created for readability purposes only.
        /// </remarks>
        private string GetOperationSystemVersionFromEnvironment()
        {
            var version = Environment.OSVersion.Version.ToString();
            var servicePack = Environment.OSVersion.ServicePack;

            return version + (" " + servicePack).TrimEnd();
        }

        /// <summary>
        /// Gets the device OEM.
        /// </summary>
        /// <returns>
        /// The discovered OEM.
        /// </returns>
        public async Task<string> GetOemName()
        {
            return await Task.Run(() => GetSystemManufacturer());
        }

        /// <summary>
        /// Gets the device model.
        /// </summary>
        /// <returns>
        /// The discovered device model.
        /// </returns>
        public string GetDeviceModel()
        {
            return GetValue(() => wmiService.GetManagementProperty<string>("Win32_ComputerSystem", "Model"), "Unknown");
        }

        /// <summary>
        /// Gets the network type.
        /// </summary>
        /// <returns>
        /// The discovered network type.
        /// </returns>
        public int GetNetworkType()
        {
            return GetValue(() => (int)GetActiveNetworkInterface().NetworkInterfaceType, 0);
        }

        /// <summary>
        /// Gets the host system locale.
        /// </summary>
        /// <returns>
        /// The discovered locale.
        /// </returns>
        public string GetHostSystemLocale()
        {
            return CultureInfo.CurrentCulture.Name;
        }

        /// <summary>
        /// Gets operating system name.
        /// </summary>
        /// <returns>
        /// Operating system name.
        /// </returns>
        public string GetOperatingSystemName()
        {
            return "Windows";
        }

        /// <summary>
        /// Gets the active network interface.
        /// </summary>
        /// <returns></returns>
        private NetworkInterface GetActiveNetworkInterface()
        {
            return NetworkInterface.GetAllNetworkInterfaces().Where(x => x.OperationalStatus == OperationalStatus.Up).FirstOrDefault();
        }

        /// <summary>
        /// Get the processor architecture of this computer.
        /// </summary>
        /// <returns>The processor architecture of this computer.</returns>
        public static ushort GetProcessorArchitecture()
        {
            try
            {
                var sysInfo = new NativeMethods._SYSTEM_INFO();
                NativeMethods.GetNativeSystemInfo(ref sysInfo);
                return sysInfo.wProcessorArchitecture;
            }
            catch
            {
                // unknown architecture.
                return 0xffff;
            }
        }
    }
}