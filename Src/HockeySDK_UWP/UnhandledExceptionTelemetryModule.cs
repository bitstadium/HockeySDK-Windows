namespace Microsoft.HockeyApp.Extensibility.Windows
{
    using System.Threading.Tasks;
    using System.Windows;
    using Microsoft.HockeyApp.Extensibility.Implementation.Platform;

    using global::Windows.UI.Core;
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
