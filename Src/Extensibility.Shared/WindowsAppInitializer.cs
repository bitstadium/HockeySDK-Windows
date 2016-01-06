namespace Microsoft.HockeyApp
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Channel;
    using Extensibility;
    using Extensibility.Implementation;
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
        private static WindowsCollectors collectors;
        private static TimeSpan defaultDelayTime = TimeSpan.FromSeconds(2);
        private static TaskCompletionSource<TelemetryConfiguration> configurationTask = null;
        private static PageViewTelemetryModule pageViewModule = null;
        private static UnhandledExceptionTelemetryModule unhandledExceptionModule = null;
        private static string instrumentationKey = string.Empty;
        private static string endpointAddress = string.Empty;

        /// <summary>
        /// Initializes default configuration and starts automatic telemetry collection for specified WindowsCollectors flags. Must specify InstrumentationKey as a parameter or in configuration file.
        /// <param name="collectors">Enumeration flag <see cref="WindowsCollectors"/> specifying automatic collectors. By default enable all collectors.</param>
        /// </summary>
        public static Task InitializeAsync(WindowsCollectors collectors = WindowsCollectors.Metadata | WindowsCollectors.Session | WindowsCollectors.PageView)
        {
            return InitializeAsync(string.Empty, collectors, null);
        }

        /// <summary>
        /// Initializes default configuration and starts automatic telemetry collection for specified WindowsCollectors flags. Must specify InstrumentationKey as a parameter or in configuration file.
        /// <param name="instrumentationKey">InstrumentationKey obtain from http://portal.azure.com</param>
        /// <param name="endpointAddress">The HTTP address where the telemetry is sent</param>
        /// <param name="collectors">Enumeration flag <see cref="WindowsCollectors"/> specifying automatic collectors. By default enable all collectors.</param>
        /// </summary>
        public static Task InitializeAsync(string instrumentationKey, WindowsCollectors collectors = WindowsCollectors.Metadata | WindowsCollectors.Session, string endpointAddress = null)
        {
            // ToDo: Clarify whether we need to this for UWP
#if WINRT
            if (collectors.HasFlag(WindowsCollectors.PageView) || 
                collectors.HasFlag(WindowsCollectors.UnhandledException))
            {
                CoreApplicationView view = CoreApplication.GetCurrentView();
                if (view != null)
                {
                    view.Activated += ViewOnActivated;
                    configurationTask = new TaskCompletionSource<TelemetryConfiguration>();
                    defaultDelayTime = TimeSpan.FromMilliseconds(500);
                }
            }
#endif
            if (!string.IsNullOrEmpty(instrumentationKey))
            {
                WindowsAppInitializer.instrumentationKey = instrumentationKey;
            }

            if (!string.IsNullOrEmpty(endpointAddress))
            {
                WindowsAppInitializer.endpointAddress = endpointAddress;
            }

            WindowsAppInitializer.collectors = collectors;

            return Task.Delay(defaultDelayTime).ContinueWith(t => Initalize());
        }

        private static void Initalize()
        {
            TelemetryConfiguration configuration = new TelemetryConfiguration
            {
                // default value of the iKey is string.empty
                InstrumentationKey = WindowsAppInitializer.instrumentationKey
            };

            if (WindowsAppInitializer.collectors.HasFlag(WindowsCollectors.Metadata))
            {
                configuration.ContextInitializers.Add(new DeviceContextInitializer());
                configuration.ContextInitializers.Add(new ComponentContextInitializer());
                configuration.TelemetryInitializers.Add(new UserContextInitializer());
            }

            configuration.TelemetryChannel = new PersistenceChannel();
            if (!string.IsNullOrEmpty(endpointAddress)) 
            {
                configuration.TelemetryChannel.EndpointAddress = endpointAddress;
            }

            TelemetryConfigurationFactory.Instance.Initialize(configuration);
            TelemetryConfiguration.Active = configuration;

            if (WindowsAppInitializer.collectors.HasFlag(WindowsCollectors.Session))
            {
                SessionTelemetryModule sessionModule = new SessionTelemetryModule();
                sessionModule.Initialize(configuration);
                TelemetryModules.Instance.Modules.Add(sessionModule);
            }

            if (WindowsAppInitializer.collectors.HasFlag(WindowsCollectors.PageView))
            {
                LazyInitializer.EnsureInitialized(
                    ref WindowsAppInitializer.pageViewModule,
                    () => new PageViewTelemetryModule());

#if WINDOWS_PHONE
                WindowsAppInitializer.pageViewModule.Initialize(configuration);
#endif
                TelemetryModules.Instance.Modules.Add(WindowsAppInitializer.pageViewModule);
            }

            if (WindowsAppInitializer.collectors.HasFlag(WindowsCollectors.UnhandledException))
            {
                LazyInitializer.EnsureInitialized(
                    ref WindowsAppInitializer.unhandledExceptionModule,
                    () => new UnhandledExceptionTelemetryModule());
#if WINDOWS_PHONE || WINDOWS_UWP
                WindowsAppInitializer.unhandledExceptionModule.Initialize(configuration);
#endif
                TelemetryModules.Instance.Modules.Add(WindowsAppInitializer.unhandledExceptionModule);
            }

            if (WindowsAppInitializer.configurationTask != null)
            {
                WindowsAppInitializer.configurationTask.TrySetResult(configuration);
            }
        }

#if WINRT
        private static void ViewOnActivated(CoreApplicationView sender, IActivatedEventArgs args)
        {
            // Waiting that the initialization of the module and configuration is done before initializing the modules
            TelemetryConfiguration configuration = WindowsAppInitializer.configurationTask.Task.ConfigureAwait(false).GetAwaiter().GetResult();
            try
            {
                CoreDispatcher dispatcher = sender.Dispatcher;

                if (WindowsAppInitializer.pageViewModule != null)
                {
                    Task donAwaitTask = WindowsAppInitializer.pageViewModule.InitializeAsync(dispatcher);
                }

                if (WindowsAppInitializer.unhandledExceptionModule != null)
                {
                    Task donAwaitTask = WindowsAppInitializer.unhandledExceptionModule.InitializeAsync(dispatcher);
                }

                sender.Activated -= ViewOnActivated;
            }
            catch (COMException)
            {
                // Catching COMException that can be thrown from the UI dispatcher
            }
        }
#endif
    }
}
