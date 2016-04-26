namespace Microsoft.HockeyApp.Services
{
    using System;

    interface IApplication
    {
        event EventHandler OnResuming;

        event EventHandler OnSuspending;

        event EventHandler OnCrashed;

        void Init();

        /// <summary>
        /// Indicates whether the package is installed in development mode.
        /// </summary>
        /// <returns>True if a package is installed in development mode, otherwise false.</returns>
        bool IsDevelopmentMode();
    }
}
