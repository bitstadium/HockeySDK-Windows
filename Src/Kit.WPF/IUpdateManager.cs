namespace Microsoft.HockeyApp
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Update manager interface.
    /// </summary>
    public interface IUpdateManager
    {
        /// <summary>
        /// Check for available updates synchronously. The version information has to be in the format d*.d*[.d*.[d*]].
        /// If System.Version.TryParse() fails, no Version compare can be executed
        /// </summary>
        /// <param name="autoShowUi">Use the default update dialogs</param>
        /// <param name="shutdownActions">Callback to gracefully stop your application. If using default-ui, call has to be provided.</param>
        /// <param name="updateAvailableAction">Callback for available versions, if you want to provide own update dialogs</param>
        void CheckForUpdates(bool autoShowUi, Func<bool> shutdownActions = null, Action<IAppVersion> updateAvailableAction = null);

        /// <summary>
        /// Check for available updates asynchronously. The version information has to be in the format d*.d*[.d*.[d*]].
        /// If System.Version.TryParse() fails, no Version compare can be executed
        /// </summary>
        /// <param name="autoShowUi">Use the default update dialogs</param>
        /// <param name="shutdownActions">The shutdown actions.</param>
        /// <param name="updateAvailableAction">Callback for available versions, if you want to provide own update dialogs</param>
        /// <returns>false if no new version is found</returns>
        Task<bool> CheckForUpdatesAsync(bool autoShowUi, Func<bool> shutdownActions = null, Action<IAppVersion> updateAvailableAction = null);
    }
}
