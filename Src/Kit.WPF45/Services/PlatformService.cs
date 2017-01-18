namespace Microsoft.HockeyApp.Services
{
    using System;
    using System.Collections.Generic;
    using Extensibility;
    using Extensibility.Implementation.External;
    using System.Diagnostics;
    using Microsoft.HockeyApp.Extensibility.Implementation;
    using System.IO;
    using System.Reflection;
    using System.IO.IsolatedStorage;
    using System.Collections;
    using System.Runtime.Serialization.Formatters.Binary;

    /// <summary>
    /// The platform service
    /// </summary>
    internal class PlatformService : IPlatformService
    {
        /// <summary>
        /// Default implementation for debug output interface
        /// </summary>
        private class DebugOutput : IDebugOutput
        {
            /// <summary>
            /// Checks to see if logging is enabled by an attached debugger.
            /// </summary>
            /// <returns>
            /// true if a debugger is attached and logging is enabled; otherwise, false.
            /// </returns>
            public bool IsLogging() { return Debugger.IsLogging(); }

            /// <summary>
            /// Write the message to the VisualStudio output window.
            /// </summary>
            /// <param name="message"></param>
            public void WriteLine(string message) { Debug.WriteLine(message); }
        }

        /// <summary>
        /// The debug output
        /// </summary>
        private DebugOutput debugOutput;

        /// <summary>
        /// The local settings
        /// </summary>
        private PersistentDictionary<string, object> localSettings;

        /// <summary>
        /// The roaming settings
        /// </summary>
        private PersistentDictionary<string, object> roamingSettings;

        /// <summary>
        /// The synchronize root
        /// </summary>
        private object syncRoot = new object();

        /// <summary>
        /// Returns the platform specific Debugger writer to the VS output console.
        /// </summary>
        /// <returns></returns>
        public IDebugOutput GetDebugOutput()
        {
            if (debugOutput == null)
                debugOutput = new DebugOutput();

            return debugOutput;
        }

        /// <summary>
        /// Returns the platform specific <see cref="ExceptionDetails" /> object for the given Exception.
        /// </summary>
        /// <param name="exception">The exception</param>
        /// <param name="parentExceptionDetails">The parent's exception details</param>
        /// <returns>The exception details</returns>
        public ExceptionDetails GetExceptionDetails(Exception exception, ExceptionDetails parentExceptionDetails)
        {
            return ExceptionConverter.ConvertToExceptionDetails(exception, parentExceptionDetails);
        }

        /// <summary>
        /// Use this locality for data you need to persist. The system will never destroy whatever data you put here unless the user uninstalls the package.
        /// </summary>
        /// <returns>The local settings</returns>
        public IDictionary<string, object> GetLocalApplicationSettings()
        {
            if (localSettings == null)
            {
                lock (syncRoot)
                {
                    if (localSettings == null)
                    {
                        localSettings = new PersistentDictionary<string, object>("local.settings");
                    }
                }
            }

            return localSettings;
        }

        /// <summary>
        /// Use this locality for data you want the system to automatically copy across the user's PCs. Windows Store apps are licensed to a user, and a user is allowed
        /// to install a single app on many PCs. This locality causes your package's data to be the same across all the user's PCs using an eventual consistency model.
        /// </summary>
        /// <returns>The roaming settings</returns>
        public IDictionary<string, object> GetRoamingApplicationSettings()
        {
            if (roamingSettings == null)
            {
                lock (syncRoot)
                {
                    if (roamingSettings == null)
                    {
                        roamingSettings = new PersistentDictionary<string, object>("roaming.settings");
                    }
                }
            }

            return roamingSettings;
        }

        /// <summary>
        /// Returns contents of the configuration file in the application directory.
        /// </summary>
        /// <returns>The configuration xml</returns>
        public string ReadConfigurationXml()
        {
            var path = Path.Combine(Assembly.GetEntryAssembly().Location, "HockeyApp.config");

            if (File.Exists(path))
            {
                var stream = File.OpenRead(path);
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Name of SDK as it appears on http://nuget.org, for example
        /// for https://www.nuget.org/packages/HockeySDK.UWP/ it is HockeySDK.UWP
        /// </summary>
        /// <returns>The name of the SDK</returns>
        public string SdkName()
        {
            return "HockeySDK.WPF";
        }
    }
}
