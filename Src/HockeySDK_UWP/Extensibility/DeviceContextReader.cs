namespace Microsoft.HockeyApp.Extensibility
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    using Implementation.Platform;

    using global::Windows.ApplicationModel.Core;
    using global::Windows.Graphics.Display;
    using global::Windows.Networking.Connectivity;
    using global::Windows.Security.Cryptography;
    using global::Windows.Security.Cryptography.Core;
    using global::Windows.Storage.Streams;
    using global::Windows.System.Profile;

    /// <summary>
    /// The reader is platform specific and applies to Windows Phone WinRT applications only.
    /// </summary>
    internal partial class DeviceContextReader // : IDeviceContextReader
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
        /// The current application resolution.
        /// </summary>
        private string screenResolution;

        /// <summary>
        /// The network type.
        /// </summary>
        private int? networkType;

        ///// <summary>
        ///// Gets the fallback device context.
        ///// </summary>
        //public virtual FallbackDeviceContext FallbackContext
        //{
        //    get { return null; }
        //}

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
            PropertyInfo propertyInfo = System.Reflection.TypeExtensions.GetProperties(typeof(DisplayInformation))
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

                    if ((hardwareEntry[0] == 1 ||  // CPU ID of the processor
                         hardwareEntry[0] == 2 ||  // Size of the memory
                         hardwareEntry[0] == 3 ||  // Serial number of the disk device
                         hardwareEntry[0] == 7 ||  // Mobile broadband ID
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
            IBuffer clearBuffer = CryptographicBuffer.ConvertStringToBinary(builder.ToString(), BinaryStringEncoding.Utf8);
            
            // get a provider for the SHA256 algorithm
            HashAlgorithmProvider hashAlgorithmProvider = HashAlgorithmProvider.OpenAlgorithm("SHA256");
            
            // hash the input buffer
            IBuffer hashedBuffer = hashAlgorithmProvider.HashData(clearBuffer);

            return this.deviceId = CryptographicBuffer.EncodeToBase64String(hashedBuffer);
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

        public async Task<string> GetOemName()
        {
            return await PlatformHacks.GetDeviceManufacturerAsync();
        }

        /// <summary>
        /// Gets the device model.
        /// </summary>
        /// <returns>The discovered device model.</returns>
        public async Task<string> GetDeviceModel()
        {
            return await PlatformHacks.GetDeviceModelAsync();
        }

        public async Task<string> GetScreenResolutionAsync()
        {
            if (this.screenResolution == null)
            {
                this.screenResolution = await GetScreenResolutionInternal();
            }

            return this.screenResolution;
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
            if (profile == null ||
                profile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.None)
            {
                this.networkType = 0;
                return this.networkType.Value;
            }

            this.networkType = (int)profile.NetworkAdapter.IanaInterfaceType;
            return this.networkType.Value;
        }
        
        private static async Task<string> GetScreenResolutionInternal()
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
                    PropertyInfo propertyInfo = System.Reflection.TypeExtensions.GetProperties(displayInformation.GetType())
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
    }
}