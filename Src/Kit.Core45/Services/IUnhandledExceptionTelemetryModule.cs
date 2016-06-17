namespace Microsoft.HockeyApp.Services
{
    using Channel;
    using DataContracts;
    using Extensibility;
    using System;

    internal interface IUnhandledExceptionTelemetryModule : ITelemetryModule
    {
        ITelemetry CreateCrashTelemetry(Exception exception, ExceptionHandledAt handledAt);
    }
}
