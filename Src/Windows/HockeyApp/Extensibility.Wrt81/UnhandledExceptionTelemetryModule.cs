namespace Microsoft.ApplicationInsights.Extensibility.Windows
{
    using System.Threading.Tasks;
    using global::Windows.UI.Core;
    using global::Windows.UI.Xaml;

    /// <summary>
    /// Unhandled exception telemetry module for WinRT.
    /// </summary>
    public sealed partial class UnhandledExceptionTelemetryModule
    {
        internal Task InitializeAsync(CoreDispatcher dispatcher = null)
        {
            return PlatformDispatcher.RunAsync(
                () => Application.Current.UnhandledException += this.ApplicationOnUnhandledException,
                dispatcher);
        }
    }
}
