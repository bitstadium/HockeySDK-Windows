namespace Microsoft.HockeyApp.Services
{
    using System;
    using System.Threading;

    using Channel;

    using DataContracts;
    using Microsoft.HockeyApp.Extensibility.Implementation.External;
    using System.Windows;

    /// <summary>
    /// This module is obviously not doing much, unhandled exception handlers are attached elsewhere
    /// </summary>
    internal sealed class UnhandledExceptionTelemetryModule : IUnhandledExceptionTelemetryModule
    {
        /// <summary>
        /// Initialize method is called after all configuration properties have been loaded from the configuration.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Creates the crash telemetry.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="handledAt">The handled at information.</param>
        /// <returns>The exception telemetry</returns>
        public ITelemetry CreateCrashTelemetry(Exception exception, ExceptionHandledAt handledAt)
        {
            throw new NotImplementedException();
        }
    }
}