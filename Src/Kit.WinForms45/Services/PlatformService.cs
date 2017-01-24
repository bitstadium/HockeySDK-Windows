using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

using Microsoft.HockeyApp.Extensibility;
using Microsoft.HockeyApp.Extensibility.Implementation.External;
using Microsoft.HockeyApp.Extensibility.Implementation;

namespace Microsoft.HockeyApp.Services
{
    sealed class PlatformService : IPlatformService
    {
        private sealed class DebugOutput : IDebugOutput
        {
            public bool IsLogging() => Debugger.IsLogging();

            public void WriteLine(string message) => Debug.WriteLine(message);
        }

        public PlatformService(IDictionary<string,object> localApplicationSettings, IDictionary<string, object> roamingApplicationSettings)
        {
            if (localApplicationSettings == null) { throw new ArgumentNullException("localApplicationSettings"); }
            if (roamingApplicationSettings == null) { throw new ArgumentNullException("roamingApplicationSettings"); }

            _localSettings = localApplicationSettings;
            _roamingSettings = roamingApplicationSettings;
        }

        private DebugOutput _debug;
        public IDebugOutput GetDebugOutput()
        {
            return _debug ?? (_debug = new DebugOutput());
        }

        public ExceptionDetails GetExceptionDetails(Exception exception, ExceptionDetails parentExceptionDetails)
        {
            return ExceptionConverter.ConvertToExceptionDetails(exception, parentExceptionDetails);
        }

        private readonly IDictionary<string, object> _localSettings;
        public IDictionary<string, object> GetLocalApplicationSettings()
        {
            return _localSettings;
        }

        private readonly IDictionary<string, object> _roamingSettings;
        public IDictionary<string, object> GetRoamingApplicationSettings()
        {
            return _roamingSettings;
        }

        public string ReadConfigurationXml()
        {
            var path = Path.Combine(Assembly.GetEntryAssembly().Location, "HockeyApp.config");
            if (File.Exists(path))
            {
                Stream stream = File.OpenRead(path);
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }

            return string.Empty;
        }

        public string SdkName()
        {
            return "HockeySDK.WinForms";
        }
    }
}
