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
        /// Initializes the service.
        /// </summary>
        void Init();

        /// <summary>
        /// Indicates whether the application is installed in development mode.
        /// </summary>
        /// <returns>True if a package is installed in development mode, otherwise false.</returns>
        bool IsDevelopmentMode();

        /// <summary>
        /// Gets the version for the current application. If the version cannot be found, we will return the passed in default.
        /// </summary>
        /// <returns>The extracted data.</returns>
        string GetVersion();

        /// <summary>
        /// Gets the application id, which is then namespace name for App class.
        /// </summary>
        /// <returns>Namespace name for App class.</returns>
        string GetApplicationId();

        /// <summary>
        /// Gets the application store region.
        /// </summary>
        /// <returns>The two-letter identifier for the user's region.</returns>
        string GetStoreRegion();
    }
}
