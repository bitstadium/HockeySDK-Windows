namespace Microsoft.HockeyApp.Extensibility
{
    using System;
    using System.Globalization;
    using System.Threading.Tasks;
    using DataContracts;
    

    /// <summary>
    /// A telemetry context initializer that will gather device context information.
    /// </summary>
    public class DeviceContextInitializer : IContextInitializer
    {
        private static string DeviceModel = null;
        private static string DeviceManufacturer = null;
        private static string OperatingSystem = null;

        /// <summary>
        /// Initializes the given <see cref="TelemetryContext" />.
        /// </summary>
        /// <param name="context">The telemetry context to initialize.</param>
        public async void Initialize(TelemetryContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            IDeviceContextReader reader = DeviceContextReader.Instance;
            context.Device.Type = reader.GetDeviceType();
            context.Device.Id = reader.GetDeviceUniqueId();

            await InitializeOperatingSystem(reader, context);
            await InitializeDeviceManufacturer(reader, context);
            await InitializeDeviceModel(reader, context);

            context.Device.NetworkType = reader.GetNetworkType().ToString(CultureInfo.InvariantCulture);
            context.Device.Language = reader.GetHostSystemLocale();
        }

        private async Task InitializeDeviceManufacturer(IDeviceContextReader reader, TelemetryContext context)
        {
            if (DeviceManufacturer == null)
            {
                DeviceManufacturer = await reader.GetOemName();
            }

            context.Device.OemName = DeviceManufacturer;
        }

        private async Task InitializeDeviceModel(IDeviceContextReader reader, TelemetryContext context)
        {
            if (DeviceModel == null)
            {
                DeviceModel = await reader.GetDeviceModel();
            }

            context.Device.Model = DeviceModel;
        }

        private async Task InitializeOperatingSystem(IDeviceContextReader reader, TelemetryContext context)
        {
            if (OperatingSystem == null)
            {
                OperatingSystem = await reader.GetOperatingSystemAsync();
            }

            context.Device.OperatingSystem = OperatingSystem;
        }
    }
}
