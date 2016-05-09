namespace Microsoft.HockeyApp.Extensibility
{
    using System;
    using System.Threading.Tasks;
    using DataContracts;
    using Services;
    
    /// <summary>
    /// A telemetry context initializer that will gather component context information.
    /// </summary>
    internal class ComponentContextInitializer : IContextInitializer
    {
#pragma warning disable 1998
        /// <summary>
        /// Initializes the given <see cref="TelemetryContext" />.
        /// </summary>
        /// <param name="context">The telemetry context to initialize.</param>
        public async Task Initialize(TelemetryContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            var applicationService = ServiceLocator.GetService<IApplicationService>();
            applicationService.Init();
            context.Component.Version = applicationService.GetVersion();
            context.Component.ApplicationId = applicationService.GetApplicationId();
        }
#pragma warning restore 1998
    }
}
