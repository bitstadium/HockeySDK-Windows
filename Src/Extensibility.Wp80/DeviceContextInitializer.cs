namespace Microsoft.HockeyApp.Extensibility
{
    using System;
    using System.Globalization;
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
        public void Initialize(TelemetryContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            IDeviceContextReader reader = DeviceContextReader.Instance;
            context.Device.Type = reader.GetDeviceType();
            context.Device.Id = reader.GetDeviceUniqueId();

            InitializeOperatingSystem(reader, context);
            InitializeDeviceManufacturer(reader, context);
            InitializeDeviceModel(reader, context);

            context.Device.NetworkType = reader.GetNetworkType().ToString(CultureInfo.InvariantCulture);
            context.Device.Language = reader.GetHostSystemLocale();
        }

        private void InitializeDeviceManufacturer(IDeviceContextReader reader, TelemetryContext context)
        {
            if (DeviceManufacturer == null)
            {
                reader.GetOemName().ContinueWith(
                    task =>
                    {
                        if (task.IsCompleted)
                        {
                            DeviceManufacturer = task.Result;
                            context.Device.OemName = DeviceManufacturer;
                        }
                    }
                );
            }
            else
            {
                context.Device.OemName = DeviceManufacturer;
            }
        }

        private void InitializeDeviceModel(IDeviceContextReader reader, TelemetryContext context)
        {
            if (DeviceModel == null)
            {
                reader.GetDeviceModel().ContinueWith(
                    task =>
                    {
                        if (task.IsCompleted)
                        {
                            DeviceModel = task.Result;
                            context.Device.Model = DeviceModel;
                        }
                    }
                );
            }
            else
            {
                context.Device.Model = DeviceModel;
            }
        }

        private void InitializeOperatingSystem(IDeviceContextReader reader, TelemetryContext context)
        {
            if (OperatingSystem == null)
            {
                reader.GetOperatingSystemAsync().ContinueWith(
                 task =>
                 {
                     if (task.IsCompleted)
                     {
                         OperatingSystem = task.Result;
                         context.Device.OperatingSystem = OperatingSystem;
                     }
                 });
            }
            else
            {
                context.Device.OperatingSystem = OperatingSystem;
            }
        }
    }
}
