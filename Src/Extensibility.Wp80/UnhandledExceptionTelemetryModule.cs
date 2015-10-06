namespace Microsoft.ApplicationInsights.Extensibility.Windows
{
    using System.Threading.Tasks;
    using System.Windows;

    using global::Windows.UI.Core;

    public sealed partial class UnhandledExceptionTelemetryModule
    {
        internal Task InitializeAsync()
        {
            return PlatformDispatcher.RunAsync(() => Application.Current.UnhandledException += this.ApplicationOnUnhandledException);
        }
    }
}
