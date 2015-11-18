namespace Microsoft.HockeyApp.Extensibility
{
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using DataContracts;

    /// <summary>
    /// Initializes SDK Properties: SDK Version and SDKMode.
    /// </summary>
    internal sealed class SdkVersionPropertyContextInitializer : IContextInitializer
    {
        private const string SDKVersion = "SDKVersion";
        private string sdkVersion;

#pragma warning disable 1998
        /// <summary>
        /// Adds a telemetry property for the version of SDK.
        /// </summary>
        public async Task Initialize(TelemetryContext context)
        {
            var version = LazyInitializer.EnsureInitialized(ref this.sdkVersion, this.GetAssemblyVersion);
            if (string.IsNullOrEmpty(context.Internal.SdkVersion))
            {
                context.Internal.SdkVersion = version;
            }
        }
#pragma warning restore 1998

        private string GetAssemblyVersion()
        {
#if !WINRT && !CORE_PCL && !UWP
            return typeof(SdkVersionPropertyContextInitializer).Assembly.GetCustomAttributes(false)
                    .OfType<AssemblyFileVersionAttribute>()
                    .First()
                    .Version;
#else
            return typeof(SdkVersionPropertyContextInitializer).GetTypeInfo().Assembly.GetCustomAttributes<AssemblyFileVersionAttribute>()
                    .First()
                    .Version;
#endif
        }
    }
}