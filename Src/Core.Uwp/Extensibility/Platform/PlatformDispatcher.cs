namespace Microsoft.HockeyApp.Extensibility.Implementation.Platform
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.HockeyApp.Extensibility.Implementation.Tracing;

    using Windows.ApplicationModel.Activation;
    using Windows.ApplicationModel.Core;
    using Windows.UI.Core;

    internal class PlatformDispatcher : IPlatformDispatcher
    {
        private TaskCompletionSource<CoreDispatcher> dispathcerCompletionSource;

        public Task RunAsync(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>();
            Task<CoreDispatcher> task = this.GetDispatcherSafely();

            // Once the view will get active we will run the action on the dispacher
            Task.Factory.StartNew(
                async () =>
                    {
                        CoreDispatcher dispatcher = await task;
                        await dispatcher.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(action));
                        taskCompletionSource.TrySetResult(null);
                    })
                    .ContinueWith(prev => CoreEventSource.Log.LogVerbose(prev.Exception.ToString()), TaskContinuationOptions.OnlyOnFaulted);

            return taskCompletionSource.Task;
        }
        
        protected virtual Task<CoreDispatcher> GetDispatcherSafely()
        {
            if (this.dispathcerCompletionSource != null)
            {
                return this.dispathcerCompletionSource.Task;
            }

            TaskCompletionSource<CoreDispatcher> taskCompletionSource = new TaskCompletionSource<CoreDispatcher>();
            Interlocked.CompareExchange(ref this.dispathcerCompletionSource, taskCompletionSource, null);
            
            // Subsribe to the activate event of the view, once the view is active we can access the dispatcher safely
            CoreApplicationView view = CoreApplication.GetCurrentView();
            view.Activated += this.OnViewActivated;

            return this.dispathcerCompletionSource.Task;
        }

        private void OnViewActivated(CoreApplicationView sender, IActivatedEventArgs args)
        {
            // passing the current view dispatcher
            this.dispathcerCompletionSource.TrySetResult(sender.Dispatcher);
        }
    }
}
