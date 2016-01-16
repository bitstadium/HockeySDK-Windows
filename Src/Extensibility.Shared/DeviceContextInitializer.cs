namespace Microsoft.HockeyApp.Extensibility
{
    using System;
    using System.Globalization;
    using System.Threading.Tasks;
    using DataContracts;

    /// <summary>
    /// A telemetry context initializer that will gather device context information.
    /// </summary>
    internal class DeviceContextInitializer : IContextInitializer
    {
        private static string deviceType = null;
        private static string deviceId = null;
        private static string deviceModel = null;
        private static string language = null;
        private static string deviceOS = null;
        private static string deviceManufacturer = null;
        private static string operatingSystemVersion = null;
        private static string networkType = null;

        /// <summary>
        /// Initializes the given <see cref="TelemetryContext" />.
        /// </summary>
        /// <param name="context">The telemetry context to initialize.</param>
        public async Task Initialize(TelemetryContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            var reader = new DeviceContextReader();
            await this.InitializeDeviceOSVersion(reader, context);
            await this.InitializeDeviceManufacturer(reader, context);
            await this.InitializeDeviceType(reader, context);
            this.InitializeDeviceModel(reader, context);
            this.InitializeDeviceId(reader, context);
            this.InitializeNetworkType(reader, context);
            this.InitializeDeviceOS(reader, context);
        }

        private async Task InitializeDeviceType(IDeviceContextReader reader, TelemetryContext context)
        {
            if (deviceType == null)
            {
                deviceType = await reader.GetDeviceType();
            }

            context.Device.Type = deviceType;
        }

        private void InitializeDeviceId(IDeviceContextReader reader, TelemetryContext context)
        {
            if (deviceId == null)
            {
                deviceId = reader.GetDeviceUniqueId();
            }

            context.Device.Id = deviceId;
        }

        private void InitializeNetworkType(IDeviceContextReader reader, TelemetryContext context)
        {
            if (networkType == null)
            {
                networkType = reader.GetNetworkType().ToString(CultureInfo.InvariantCulture);
            }

            context.Device.NetworkType = networkType;
        }

        private void InitializeLanguage(IDeviceContextReader reader, TelemetryContext context)
        {
            if (language == null)
            {
                language = reader.GetHostSystemLocale();
            }

            context.Device.Language = language;
        }

        private void InitializeDeviceOS(IDeviceContextReader reader, TelemetryContext context)
        {
            if (deviceOS == null)
            {
                deviceOS = reader.GetOperatingSystemName();
            }

            context.Device.DeviceOS = deviceOS;
        }

        private async Task InitializeDeviceManufacturer(IDeviceContextReader reader, TelemetryContext context)
        {
            if (deviceManufacturer == null)
            {
                deviceManufacturer = await reader.GetOemName();
            }

            context.Device.OemName = deviceManufacturer;
        }

        private void InitializeDeviceModel(IDeviceContextReader reader, TelemetryContext context)
        {
            if (deviceModel == null)
            {
                deviceModel = reader.GetDeviceModel();
            }

            context.Device.Model = deviceModel;
        }

        private async Task InitializeDeviceOSVersion(IDeviceContextReader reader, TelemetryContext context)
        {
            if (operatingSystemVersion == null)
            {
                operatingSystemVersion = await reader.GetOperatingSystemVersionAsync();
            }

            context.Device.DeviceOSVersion = operatingSystemVersion;
        }
    }
}
