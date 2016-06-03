namespace Microsoft.HockeyApp.Services
{
    using System;
    using System.Collections.Generic;
    using Extensibility;
    using Extensibility.Implementation.External;

    class PlatformService : IPlatformService
    {
        public IDebugOutput GetDebugOutput()
        {
            throw new NotSupportedException();
        }

        public ExceptionDetails GetExceptionDetails(Exception exception, ExceptionDetails parentExceptionDetails)
        {
            throw new NotSupportedException();
        }

        public IDictionary<string, object> GetLocalApplicationSettings()
        {
            throw new NotSupportedException();
        }

        public IDictionary<string, object> GetRoamingApplicationSettings()
        {
            throw new NotSupportedException();
        }

        public string ReadConfigurationXml()
        {
            throw new NotSupportedException();
        }

        public string SdkName()
        {
            return "HockeySDK.WPF";
        }
    }
}
