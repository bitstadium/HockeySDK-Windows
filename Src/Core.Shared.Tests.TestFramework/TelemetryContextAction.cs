namespace Microsoft.ApplicationInsights.TestFramework
{
    using DataContracts;

    /// <summary>
    /// Required by Windows Runtime, which does not allow generics in public APIs.
    /// </summary>
    public delegate void TelemetryContextAction(TelemetryContext context);
}
