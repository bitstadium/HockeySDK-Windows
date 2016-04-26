namespace Microsoft.HockeyApp.Services
{
    using System;
    using global::Windows.ApplicationModel.Core;

    internal class Application : IApplication
    {
        private bool initialized = false;

        public event EventHandler OnCrashed;

        /// <summary>
        /// Occurs when an app is resuming.
        /// </summary>
        public event EventHandler OnResuming;

        /// <summary>
        /// Occurs when an app is suspending.
        /// </summary>
        public event EventHandler OnSuspending;

        public void Init()
        {
            if (initialized)
            {
                return;
            }

            CoreApplication.Resuming += CoreApplication_Resuming;
            CoreApplication.Suspending += CoreApplication_Suspending;
            initialized = true;
        }

        private void CoreApplication_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            if (OnSuspending != null)
            {
                this.OnSuspending(this, null);
            }
        }

        internal void CoreApplication_Resuming(object sender, object e)
        {
            if (OnResuming != null)
            {
                this.OnResuming(this, null);
            }
        }

        public bool IsDevelopmentMode()
        {
            return Windows.ApplicationModel.Package.Current.IsDevelopmentMode;
        }
    }
}
