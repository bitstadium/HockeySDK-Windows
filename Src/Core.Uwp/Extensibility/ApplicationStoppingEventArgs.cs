namespace Microsoft.HockeyApp.Extensibility
{
    using System;
    using System.Globalization;
    using System.Threading.Tasks;
    using Microsoft.HockeyApp.Extensibility.Implementation.Tracing;

    /// <summary>
    /// Encapsulates arguments of the <see cref="IApplicationLifecycle.Stopping"/> event.
    /// </summary>
#if WINRT
    internal
#else
    public 
#endif
    class ApplicationStoppingEventArgs : EventArgs
    {
        internal static new readonly ApplicationStoppingEventArgs Empty = new ApplicationStoppingEventArgs(asyncMethod => asyncMethod());

        private readonly Func<Func<Task>, Task> asyncMethodRunner;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationStoppingEventArgs"/> class with the specified runner of asynchronous methods.
        /// </summary>
        public ApplicationStoppingEventArgs(Func<Func<Task>, Task> asyncMethodRunner)
        {
            if (asyncMethodRunner == null)
            {
                throw new ArgumentNullException("asyncMethodRunner");
            }

            this.asyncMethodRunner = asyncMethodRunner;
        }

        /// <summary>
        /// Runs the specified asynchronous method while preventing the application from exiting.
        /// </summary>
        public async void Run(Func<Task> asyncMethod)
        {
            try
            {
                await this.asyncMethodRunner(asyncMethod);
            }
            catch (Exception exception)
            {
                string message = string.Format(
                    CultureInfo.InvariantCulture,
                    "Unexpected excption when handling IApplicationLifecycle.Stopping event:{0}",
                    exception.ToString());
                CoreEventSource.Log.LogError(message);
            }            
        }
    }
}
