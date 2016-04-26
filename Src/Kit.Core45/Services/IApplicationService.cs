namespace Microsoft.HockeyApp.Services
{
    using System;

    interface IApplicationService
    {
        /// <summary>
        /// Occurs when an app is suspending.
        /// </summary>
        event EventHandler OnSuspending;

        /// <summary>
        /// Occurs when an app is resuming.
        /// </summary>

        event EventHandler OnResuming;

        /// <summary>
        /// Occurs when an exception that is raised by application is not handled.
        /// </summary>
        event EventHandler OnCrashed;

        /// <summary>
        /// Initializes the service.
        /// </summary>
        void Init();

        /// <summary>
        /// Indicates whether the application is installed in development mode.
        /// </summary>
        /// <returns>True if a package is installed in development mode, otherwise false.</returns>
        bool IsDevelopmentMode();
    }
}
