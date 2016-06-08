namespace Microsoft.HockeyApp.Extensibility
{
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using DataContracts;

    /// <summary>
    /// Initializes SDK Properties: SDK Version and SDKMode.
    /// </summary>
    internal sealed class SdkVersionPropertyContextInitializer : IContextInitializer
    {
        private static string sdkVersion;

#pragma warning disable 1998
        /// <summary>
        /// Adds a telemetry property for the version of SDK.
        /// </summary>
        public async Task Initialize(TelemetryContext context)
        {
            context.Internal.SdkVersion = GetAssemblyVersion();
        }
#pragma warning restore 1998

        internal static string GetAssemblyVersion()
        {
            if (sdkVersion == null)
            {
                var platformService = ServiceLocator.GetService<Services.IPlatformService>();

                // SDK Name is expected to be lower case
                sdkVersion = platformService.SdkName().ToLowerInvariant() + ":" + typeof(SdkVersionPropertyContextInitializer).GetTypeInfo().Assembly.GetCustomAttributes<AssemblyFileVersionAttribute>()
                    .First()
                    .Version;
            }

            return sdkVersion;
        }
    }
}