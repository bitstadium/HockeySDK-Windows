namespace Microsoft.ApplicationInsights.Extensibility
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    using Extensibility.Implementation;
    using Extensibility.Implementation.Tracing;

    using global::Windows.ApplicationModel.Core;
    using global::Windows.Graphics.Display;
    using global::Windows.Networking.Connectivity;
    using global::Windows.Security.Cryptography;
    using global::Windows.Security.Cryptography.Core;
    using global::Windows.Security.ExchangeActiveSyncProvisioning;
    using global::Windows.Storage.Streams;
    using global::Windows.System.Profile;

    /// <summary>
    /// The reader is platform specific and applies to Windows Phone WinRT applications only.
    /// </summary>
    internal partial class DeviceContextReader : IDeviceContextReader
    {
        /// <summary>
        /// The number of milliseconds to wait before asynchronously retrying an operation.
        /// </summary>
        private const int AsyncRetryIntervalInMilliseconds = 100;

        /// <summary>
        /// The type of the device.
        /// </summary>
        private string deviceType;

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
            if (this.deviceType != null)
            {
                return this.deviceType;
            }

            // The RawPixelsPerViewPixel property only exists on phone devices
            PropertyInfo propertyInfo = typeof(DisplayInformation)
                                                   .GetProperties()
                                                   .FirstOrDefault(item => string.Compare(item.Name, "RawPixelsPerViewPixel", StringComparison.Ordinal) == 0);
            
            this.deviceType = propertyInfo != null ? "Phone" : "Other";
            return this.deviceType;
        }

        /// <summary>
        /// Gets the device unique identifier.
        /// </summary>
        /// <returns>The discovered device identifier.</returns>
        public virtual string GetDeviceUniqueId()
        {
            if (this.deviceId != null)
            {
                return this.deviceId;
            }

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

                this.deviceId = CryptographicBuffer.EncodeToBase64String(hashedBuffer);
            }
            catch (Exception)
            {
                // For IoT sceanrios we will alwasy set the device id to IoT
                // Becuase HardwareIdentification API will always throw
                this.deviceId = "IoT";
            }

            return this.deviceId;
        }

        /// <summary>
        /// Gets the operating system version.
        /// </summary>
        /// <returns>The discovered operating system.</returns>
        public virtual Task<string> GetOperatingSystemAsync()
        {
            if (this.operatingSystem != null)
            {
                return Task.FromResult(this.operatingSystem);
            }

            return PlatformHacks.GetWindowsVersion()
                                .ContinueWith(
                                    previousTask =>
                                        {
                                            if (previousTask.IsFaulted == true &&
                                                previousTask.Exception != null)
                                            {
                                                throw previousTask.Exception;
                                            }

                                            return this.operatingSystem = string.Format(
                                                                                    CultureInfo.InvariantCulture,
                                                                                    "Windows NT {0}",
                                                                                    previousTask.Result);
                                        });
        }

        public virtual string GetOemName()
        {
            if (this.deviceManufacturer != null)
            {
                return this.deviceManufacturer;
            }

            try
            {
                EasClientDeviceInformation deviceInfo = new EasClientDeviceInformation();
                this.deviceManufacturer = deviceInfo.SystemManufacturer;
            }
            catch (Exception exception)
            {
                CoreEventSource.Log.LogVerbose("Fail reading Device Manufacture: " + exception.ToString());
                this.deviceManufacturer = string.Empty;
            }

            return this.deviceManufacturer;
        }

        /// <summary>
        /// Gets the device model.
        /// </summary>
        /// <returns>The discovered device model.</returns>
        public virtual string GetDeviceModel()
        {
            if (this.deviceName != null)
            {
                return this.deviceName;
            }

            try
            {
                EasClientDeviceInformation deviceInfo = new EasClientDeviceInformation();
                return this.deviceName = deviceInfo.SystemProductName;
            }
            catch (Exception exception)
            {
                CoreEventSource.Log.LogVerbose("Fail reading Device name: " + exception.ToString());
                this.deviceName = string.Empty;
                return this.deviceName;
            }
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

            try
            {
                ConnectionProfile profile = NetworkInformation.GetInternetConnectionProfile();
                if (profile == null ||
                    profile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.None)
                {
                    this.networkType = 0;
                    return this.networkType.Value;
                }

                this.networkType = (int)profile.NetworkAdapter.IanaInterfaceType;
                return this.networkType.Value;
            }
            catch (Exception exception)
            {
                CoreEventSource.Log.LogVerbose("Fail reading Device network type: " + exception.ToString());
                this.networkType = 0;
                return this.networkType.Value;
            }
        }        
    }
}