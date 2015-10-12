namespace Microsoft.ApplicationInsights.Extensibility
{
    using System;
    using System.Threading.Tasks;
    using System.Windows;
    using Extensibility.Implementation;
    using Extensibility.Implementation.Tracing;

    internal static class PlatformDispatcher
    {
        public static Task RunAsync(Action action)
        {
            if (action == null)
            {
                return Task.FromResult((object)null);
            }

            var taskCompletionSource = new TaskCompletionSource<object>();

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    action();
                    taskCompletionSource.TrySetResult(null);
                }
                catch (Exception ex)
                {
                    CoreEventSource.Log.LogError(ex.ToString());
                    taskCompletionSource.TrySetResult(null);
                }
            });

            return taskCompletionSource.Task;
        }
    }
}
