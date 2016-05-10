namespace Microsoft.HockeyApp.Extensibility
{
    using System;
    using System.Globalization;
    using System.Threading.Tasks;
    using DataContracts;
    using Services;

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

            var reader = ServiceLocator.GetService<IDeviceService>();
            await this.InitializeDeviceOSVersion(reader, context);
            await this.InitializeDeviceManufacturer(reader, context);
            await this.InitializeDeviceType(reader, context);
            this.InitializeDeviceModel(reader, context);
            this.InitializeDeviceId(reader, context);
            this.InitializeNetworkType(reader, context);
            this.InitializeDeviceOS(reader, context);
            this.InitializeLanguage(reader, context);
        }

        private async Task InitializeDeviceType(IDeviceService reader, TelemetryContext context)
        {
            if (deviceType == null)
            {
                deviceType = await reader.GetDeviceType();
            }

            context.Device.Type = deviceType;
        }

        private void InitializeDeviceId(IDeviceService reader, TelemetryContext context)
        {
            if (deviceId == null)
            {
                deviceId = reader.GetDeviceUniqueId();
            }

            context.Device.Id = deviceId;
        }

        private void InitializeNetworkType(IDeviceService reader, TelemetryContext context)
        {
            if (networkType == null)
            {
                networkType = reader.GetNetworkType().ToString(CultureInfo.InvariantCulture);
            }

            context.Device.NetworkType = networkType;
        }

        private void InitializeLanguage(IDeviceService reader, TelemetryContext context)
        {
            if (language == null)
            {
                language = reader.GetHostSystemLocale();
            }

            context.Device.Language = language;
        }

        private void InitializeDeviceOS(IDeviceService reader, TelemetryContext context)
        {
            if (deviceOS == null)
            {
                deviceOS = reader.GetOperatingSystemName();
            }

            context.Device.DeviceOS = deviceOS;
        }

        private async Task InitializeDeviceManufacturer(IDeviceService reader, TelemetryContext context)
        {
            if (deviceManufacturer == null)
            {
                deviceManufacturer = await reader.GetOemName();
            }

            context.Device.OemName = deviceManufacturer;
        }

        private void InitializeDeviceModel(IDeviceService reader, TelemetryContext context)
        {
            if (deviceModel == null)
            {
                deviceModel = reader.GetDeviceModel();
            }

            context.Device.Model = deviceModel;
        }

        private async Task InitializeDeviceOSVersion(IDeviceService reader, TelemetryContext context)
        {
            if (operatingSystemVersion == null)
            {
                operatingSystemVersion = await reader.GetOperatingSystemVersionAsync();
            }

            context.Device.DeviceOSVersion = operatingSystemVersion;
        }
    }
}
