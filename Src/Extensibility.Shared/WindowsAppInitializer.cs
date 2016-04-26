namespace Microsoft.HockeyApp
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Channel;
    using Extensibility;
    using Extensibility.Implementation;
    using Extensibility.Implementation.Tracing;
    using Extensibility.Windows;

    using global::Windows.ApplicationModel.Activation;
    using global::Windows.ApplicationModel.Core;
    using global::Windows.UI.Core;

    /// <summary>
    /// Windows app Initializer is TelemetryConfiguration and TelemetryModules 
    /// Bootstrap the WindowsApps SDK.
    /// </summary>
    internal static class WindowsAppInitializer
    {
        private static PageViewTelemetryModule pageViewModule = null;
        private static UnhandledExceptionTelemetryModule unhandledExceptionModule = null;
        private static DiagnosticsListener listener;

        /// <summary>
        /// Initializes default configuration and starts automatic telemetry collection for specified WindowsCollectors flags. Must specify InstrumentationKey as a parameter or in configuration file.
        /// <param name="instrumentationKey">Telemetry configuration.</param>
        /// </summary>
        public static Task InitializeAsync(string instrumentationKey)
        {
            return InitializeAsync(instrumentationKey, null);
        }

        /// <summary>
        /// Initializes default configuration and starts automatic telemetry collection for specified WindowsCollectors flags. Must specify InstrumentationKey as a parameter or in configuration file.
        /// <param name="instrumentationKey">Telemetry configuration.</param>
        /// <param name="configuration">Telemetry configuration.</param>
        /// </summary>
        public static Task InitializeAsync(string instrumentationKey, TelemetryConfiguration configuration)
        {
            Guid instrumentationKeyGuid;
            if (!Guid.TryParse(instrumentationKey, out instrumentationKeyGuid))
            {
                throw new ArgumentException(string.Format(System.Globalization.CultureInfo.InvariantCulture, "instrumentationKey {0} is incorrect. It must be a string representation of a GUID", instrumentationKey));
            }

            // breeze accepts instrumentation key in 32 digits separated by hyphens format only.
            instrumentationKey = instrumentationKeyGuid.ToString("D");
            return Task.Delay(TimeSpan.FromSeconds(1)).ContinueWith(t => Initalize(instrumentationKey, configuration));
        }

        private static void Initalize(string instrumentationKey, TelemetryConfiguration configuration)
        {
            if (configuration == null)
            {
                configuration = new TelemetryConfiguration();
            }

            if (configuration.EnableDiagnostics)
            {
                EnableDiagnostics();
            }

            configuration.InstrumentationKey = instrumentationKey;
            if (configuration.Collectors.HasFlag(WindowsCollectors.Metadata))
            {
                configuration.ContextInitializers.Add(new DeviceContextInitializer());
                configuration.ContextInitializers.Add(new ComponentContextInitializer());
                configuration.TelemetryInitializers.Add(new UserContextInitializer());
            }

            configuration.TelemetryChannel = new PersistenceChannel();
            if (!string.IsNullOrEmpty(configuration.EndpointAddress)) 
            {
                configuration.TelemetryChannel.EndpointAddress = configuration.EndpointAddress;
            }

            TelemetryConfigurationFactory.Instance.Initialize(configuration);
            TelemetryConfiguration.Active = configuration;

            if (configuration.Collectors.HasFlag(WindowsCollectors.Session))
            {
                SessionTelemetryModule sessionModule = new SessionTelemetryModule();
                sessionModule.Initialize(configuration);
                TelemetryModules.Instance.Modules.Add(sessionModule);
            }

            if (configuration.Collectors.HasFlag(WindowsCollectors.PageView))
            {
                LazyInitializer.EnsureInitialized(ref WindowsAppInitializer.pageViewModule, () => new PageViewTelemetryModule());

#if WINDOWS_PHONE
                WindowsAppInitializer.pageViewModule.Initialize(configuration);
#endif
                TelemetryModules.Instance.Modules.Add(WindowsAppInitializer.pageViewModule);
            }

            if (configuration.Collectors.HasFlag(WindowsCollectors.UnhandledException))
            {
                LazyInitializer.EnsureInitialized(ref WindowsAppInitializer.unhandledExceptionModule, () => new UnhandledExceptionTelemetryModule());
#if WINDOWS_PHONE || WINDOWS_UWP
                WindowsAppInitializer.unhandledExceptionModule.Initialize(configuration);
#endif
                TelemetryModules.Instance.Modules.Add(WindowsAppInitializer.unhandledExceptionModule);
            }

#if WINRT
            if (configuration.Collectors.HasFlag(WindowsCollectors.PageView) || configuration.Collectors.HasFlag(WindowsCollectors.UnhandledException))
            {
                CoreApplicationView view = CoreApplication.GetCurrentView();
                if (view != null)
                {
                    view.Activated += ViewOnActivated;
                }
            }
#endif

            HockeyClient.Current.Initialize();
        }

#if WINRT
        private static void ViewOnActivated(CoreApplicationView sender, IActivatedEventArgs args)
        {
            try
            {
                CoreDispatcher dispatcher = sender.Dispatcher;

                if (WindowsAppInitializer.pageViewModule != null)
                {
                    Task donAwaitTask = WindowsAppInitializer.pageViewModule.InitializeAsync(dispatcher);
                }

                if (WindowsAppInitializer.unhandledExceptionModule != null)
                {
                    WindowsAppInitializer.unhandledExceptionModule.Initialize(null);
                }

                sender.Activated -= ViewOnActivated;
            }
            catch (COMException)
            {
                // Catching COMException that can be thrown from the UI dispatcher
            }
        }
#endif

        private static void EnableDiagnostics()
        {
            var diagnosticSenders = new List<IDiagnosticsSender>() { new F5DiagnosticsSender() };
            listener = new DiagnosticsListener(diagnosticSenders);
        }
    }
}
