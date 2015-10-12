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
            reader.GetOperatingSystemAsync().ContinueWith(
                task =>
                    {
                        if (task.IsCompleted == true)
                        {
                            context.Device.OperatingSystem = task.Result;
                        }
                    });
            context.Device.OemName = reader.GetOemName();
            context.Device.Model = reader.GetDeviceModel();
            context.Device.NetworkType = reader.GetNetworkType().ToString(CultureInfo.InvariantCulture);
            context.Device.Language = reader.GetHostSystemLocale();
        }
    }
}
