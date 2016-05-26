namespace Microsoft.HockeyApp.Services
{
    using System;
    using System.Collections.Generic;
    using Extensibility;
    using Extensibility.Implementation.External;
    using System.IO;

    /// <summary>
    /// Encapsulates platform-specific functionality required by the API.
    /// </summary>
    /// <remarks>
    /// This type is public to enable mocking on Windows Phone.
    /// </remarks>
    internal interface IPlatformService
    {
        /// <summary>
        /// Use this locality for data you need to persist. The system will never destroy whatever data you put here unless the user uninstalls the package.
        /// </summary>
        IDictionary<string, object> GetLocalApplicationSettings();

        /// <summary>
        /// Use this locality for data you want the system to automatically copy across the user's PCs. Windows Store apps are licensed to a user, and a user is allowed
        /// to install a single app on many PCs. This locality causes your package's data to be the same across all the user's PCs using an eventual consistency model.
        /// </summary>
        IDictionary<string, object> GetRoamingApplicationSettings();

        /// <summary>
        /// Returns contents of the configuration file in the application directory.
        /// </summary>
        string ReadConfigurationXml();

        /// <summary>
        /// Returns the platform specific <see cref="ExceptionDetails"/> object for the given Exception.
        /// </summary>
        ExceptionDetails GetExceptionDetails(Exception exception, ExceptionDetails parentExceptionDetails);

        /// <summary>
        /// Returns the platform specific Debugger writer to the VS output console.
        /// </summary>
        IDebugOutput GetDebugOutput();

        /// <summary>
        /// Name of SDK as it appears on http://nuget.org, for example 
        /// for https://www.nuget.org/packages/HockeySDK.UWP/ it is HockeySDK.UWP
        /// </summary>
        /// <returns></returns>
        string SdkName();
    }
}
