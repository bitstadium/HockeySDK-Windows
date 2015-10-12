namespace Microsoft.HockeyApp.Extensibility
{
    using System;
    using System.Globalization;
    using System.Security.Cryptography;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;

    using Extensibility.Implementation.Platform;
    using Microsoft.Phone.Info;

    using global::Windows.Graphics.Display;
    using global::Windows.Networking.Connectivity;

    /// <summary>
    /// The reader is platform specific and applies to Windows Phone Silverlight applications only.
    /// </summary>
    internal partial class DeviceContextReader : IDeviceContextReader
    {
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

        /// <summary>
        /// Gets the fallback device context.
        /// </summary>
        public virtual FallbackDeviceContext FallbackContext
        {
            get
            {
                FallbackDeviceContext output = this.ReadSerializedContext();
                return output;
            }
        }

        /// <summary>
        /// Initializes the current instance with respect to the platform specific implementation.
        /// </summary>
        public void Initialize()
        {
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
        /// Gets the operating system.
        /// </summary>
        /// <returns>The discovered operating system.</returns>
        public virtual Task<string> GetOperatingSystemAsync()
        {
            if (this.operatingSystem != null)
            {
                return Task.FromResult(this.operatingSystem);
            }

            this.operatingSystem = string.Format(CultureInfo.InvariantCulture, "Windows NT {0}", Environment.OSVersion.Version.ToString(4));
            return Task.FromResult(this.operatingSystem);
        }

        /// <summary>
        /// Gets the device OEM.
        /// </summary>
        /// <returns>The discovered OEM.</returns>
        public virtual string GetOemName()
        {
            if (this.deviceManufacturer != null)
            {
                return this.deviceManufacturer;
            }

            return this.deviceManufacturer = DeviceStatus.DeviceManufacturer;
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

                FallbackDeviceContext temp = Utils.ReadSerializedContext<FallbackDeviceContext>(DeviceContextReader.ContextPersistentStorageFileName);
                Thread.MemoryBarrier();
                this.cachedContext = temp;
            }

            return this.cachedContext;
        }
    }
}