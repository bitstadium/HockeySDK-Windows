namespace Microsoft.HockeyApp.Extensibility.Windows
{
    using System.Threading.Tasks;
    using global::Windows.UI.Xaml;

    public sealed partial class UnhandledExceptionTelemetryModule
    {
        internal Task InitializeAsync()
        {
            // ToDo mihailsm: Initialize UnhandledExceptionTelemetryModule
            return PlatformDispatcher.RunAsync(() => Application.Current.UnhandledException += this.ApplicationOnUnhandledException);
        }
    }
}
