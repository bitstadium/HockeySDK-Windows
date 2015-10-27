namespace Microsoft.HockeyApp.Extensibility.Implementation.Platform
{
    using System;
    using System.Threading.Tasks;
    using Windows.ApplicationModel;
    using Windows.ApplicationModel.Core;

    /// <summary>
    /// WinRT-specific logic of <see cref="IApplicationLifecycle"/> provider.
    /// </summary>
    internal partial class PlatformApplicationLifecycle
    {
        public event Action<object, object> Started;

        public event EventHandler<ApplicationStoppingEventArgs> Stopping;

        public PlatformApplicationLifecycle()
        {
            this.Dispatcher = new PlatformDispatcher();
        }

        private IPlatformDispatcher Dispatcher { get; set; }

        internal void Initialize()
        {   
            IPlatformDispatcher platformDispatcher = this.Dispatcher;

            ExceptionHandler.Start(() => platformDispatcher.RunAsync(() => this.OnStarted(null)));
            CoreApplication.Resuming += (sender, e) => this.OnStarted(e);
            CoreApplication.Suspending += (sender, e) => this.OnStopping(new ApplicationStoppingEventArgs(RunWithDeferral(e)));        
        }

        /// <summary>
        /// Runs a function while also getting a suspension deferral from the Windows Runtime.
        /// </summary>
        private static Func<Func<Task>, Task> RunWithDeferral(ISuspendingEventArgs e)
        {
            return async function =>
            {
                ISuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();
                try
                {
                    await function();
                }
                finally
                {
                    deferral.Complete();
                }
            };
        }

        private void OnStarted(object eventArgs)
        {
            Action<object, object> handler = this.Started;
            if (handler != null)
            {
                handler(this, eventArgs);
            }
        }

        private void OnStopping(ApplicationStoppingEventArgs eventArgs)
        {
            EventHandler<ApplicationStoppingEventArgs> handler = this.Stopping;
            if (handler != null)
            {
                handler(this, eventArgs);
            }
        }

    }
}
