namespace Microsoft.HockeyApp.Extensibility.Implementation
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.HockeyApp.Extensibility.Implementation.Tracing;

    internal static class ExceptionHandler
    {
        /// <summary>
        /// Starts the <paramref name="asyncMethod"/>, catches and logs any exceptions it may throw.
        /// </summary>
        public static void Start(Func<Task> asyncMethod)
        {
            try
            {
                // Do not use await here because ASP.NET does not allow that and throws
                asyncMethod().ContinueWith(
                    task => CoreEventSource.Log.LogError("HockeySDK: An exception occured in ExceptionHandler.Start: " + task.Exception),
                    TaskContinuationOptions.OnlyOnFaulted);
            }
            catch (Exception exp)
            {
                CoreEventSource.Log.LogError(exp.ToString());
            }
        }
    }
}