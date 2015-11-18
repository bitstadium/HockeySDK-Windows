namespace Microsoft.HockeyApp.Extensibility
{
    using System;
    using System.Threading.Tasks;
    using DataContracts;

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

            var componentContextReader = ComponentContextReader.Instance;
            context.Component.Version = componentContextReader.GetVersion();
            context.Component.ApplicationId = componentContextReader.GetApplicationId();
        }
#pragma warning restore 1998
    }
}
