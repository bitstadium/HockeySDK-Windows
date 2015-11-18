namespace Microsoft.HockeyApp.Extensibility.Implementation.Platform
{
    using System;
    using System.Threading.Tasks;
    using global::Windows.ApplicationModel;
    using global::Windows.ApplicationModel.Core;

    /// <summary>
    /// WinRT-specific logic of provider.
    /// </summary>
    internal partial class PlatformApplicationLifecycle
    {
        public event Action<object, object> Started;

        public event EventHandler<ApplicationStoppingEventArgs> Stopping;

        public PlatformApplicationLifecycle()
        {
        }

        internal void Initialize()
        {   
            ExceptionHandler.Start(() => PlatformDispatcher.RunAsync(() => this.OnStarted(null)));
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
