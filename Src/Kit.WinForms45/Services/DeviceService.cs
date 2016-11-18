using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Security.Cryptography;
using System.Net.NetworkInformation;

namespace Microsoft.HockeyApp.Services
{
    enum ChassisType
    {
        Other = 1,
        Unknown = 2,
        Desktop = 3,
        LowProfileDesktop = 4,
        PizzaBox = 5,
        MiniTower = 6,
        Tower = 7,
        Portable = 8,
        Laptop = 9,
        Notebook = 10,
        HandHeld = 11,
        DockingStation = 12,
        AllInOne = 13,
        SubNotebook = 14,
        SpaceSaving = 15,
        LunchBox = 16,
        MainSystemChassis = 17,
        ExpansionChassis = 18,
        SubChassis = 19,
        BusExpansionChassis = 12,
        PeripheralChassis = 21,
        StorageChassis = 22,
        RackMountChassis = 23,
        SealedCasePC = 24
    }

    public sealed class DeviceService : IDeviceService
    {
        private readonly WmiService _wmi = new WmiService();

        public string GetSystemManufacturer()
        {
            return _wmi.GetManagementProperty("Win32_ComputerSystem", "Manufacturer");
        }

        public async Task<string> GetDeviceType()
        {
            string name = "unkown";

            try
            {
                var chassis = _wmi.GetManagementProperty("Win32_SystemEnclosure", "chassistypes");
                int type;
                if (int.TryParse(chassis, out type))
                {
                    name = Enum.GetName(typeof(ChassisType), type);
                }
            }
            catch
            {

            }

            return await Task.Run<string>(() => name);
        }

        public string GetDeviceUniqueId()
        {
            string deviceId = null;
            try
            {
                var cpus = _wmi.GetManagementProperties("win32_processor", "processorID");
                var hdds = _wmi.GetManagementProperties("Win32_LogicalDisk", "VolumeSerialNumber");
                var memory = _wmi.GetManagementProperties("Win32_ComputerSystem", "TotalPhysicalMemory");
                var biosMfg = _wmi.GetManagementProperty("Win32_BIOS", "Manufacturer");
                var biosVersion = _wmi.GetManagementProperty("Win32_BIOS", "Version");
                var biosSN = _wmi.GetManagementProperty("Win32_BIOS", "SerialNumber");

                var activeInterface = (from i in NetworkInterface.GetAllNetworkInterfaces()
                                       where i.OperationalStatus == OperationalStatus.Up
                                       select i).FirstOrDefault();

                var mac = activeInterface?.GetPhysicalAddress()?.ToString() ?? "";

                var builder = new StringBuilder();

                foreach(var cpu in cpus)
                {
                    builder.AppendFormat(CultureInfo.InvariantCulture, "{0},", cpu);
                }

                foreach (var hdd in hdds)
                {
                    builder.AppendFormat(CultureInfo.InvariantCulture, "{0},", hdd);
                }

                foreach (var size in memory)
                {
                    builder.AppendFormat(CultureInfo.InvariantCulture, "{0}", size);
                }

                builder.Append($"biosMfg,");
                builder.Append($"biosVersion,");
                builder.Append($"biosSN,");
                builder.Append(mac);

                // since we don't have access to ASHWID we're going to do our best to come up with a 
                // mostly stable, non-identifiable, but fairly unique hash of the local machine's properties

                // Per documentation here http://msdn.microsoft.com/en-us/library/windows/apps/jj553431.aspx we are selectively pulling out 
                // specific items from the hardware ID.
                var sha = SHA256.Create();
                var bytes = Encoding.UTF8.GetBytes(builder.ToString());

                var hash = sha.ComputeHash(bytes);
                deviceId = Convert.ToBase64String(hash);
            }
            catch (Exception)
            {
                // For IoT sceanrios we will alwasy set the device id to IoT
                // Becuase HardwareIdentification API will always throw
                deviceId = "IoT";
            }

            return deviceId;
        }

        public async Task<string> GetOperatingSystemVersionAsync()
        {
            return await Task.Run<string>(() => Environment.OSVersion.VersionString);
        }

        public async Task<string> GetOemName()
        {
            return await Task.Run(() => GetSystemManufacturer());
        }

        public string GetDeviceModel()
        {
            return _wmi.GetManagementProperty("Win32_ComputerSystem", "Model");
        }

        public int GetNetworkType()
        {
            var activeInterface = (from i in NetworkInterface.GetAllNetworkInterfaces()
                                   where i.OperationalStatus == OperationalStatus.Up
                                   select i).FirstOrDefault();

            return activeInterface != null ? (int)activeInterface.NetworkInterfaceType : 0;
        }

        public string GetHostSystemLocale()
        {
            return CultureInfo.CurrentCulture.Name;
        }

        public string GetOperatingSystemName()
        {
            return "Windows";
        }
    }
}