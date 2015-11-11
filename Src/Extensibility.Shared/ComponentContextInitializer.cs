namespace Microsoft.HockeyApp.Extensibility
{
    using System;
    using System.Threading.Tasks;
    using DataContracts;

    /// <summary>
    /// A telemetry context initializer that will gather component context information.
    /// </summary>
    public class ComponentContextInitializer : IContextInitializer
    {       
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

            context.Component.Version = ComponentContextReader.Instance.GetVersion();
        }
    }
}
