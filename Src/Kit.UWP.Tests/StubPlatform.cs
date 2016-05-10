namespace Microsoft.HockeyApp.TestFramework
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Extensibility;
    using Extensibility.Implementation.External;
    using Services;

    internal class StubPlatform : IPlatformService
    {
        public Func<IDictionary<string, object>> OnGetApplicationSettings = () => new Dictionary<string, object>();
        public Func<IDebugOutput> OnGetDebugOutput = () => new StubDebugOutput();
        public Func<string> OnReadConfigurationXml = () => null;
        public Func<Exception, ExceptionDetails, ExceptionDetails> OnGetExceptionDetails = (e, p) => new ExceptionDetails();

        public IDictionary<string, object> GetLocalApplicationSettings()
        {
            return this.OnGetApplicationSettings();
        }

        public IDictionary<string, object> GetRoamingApplicationSettings()
        {
            return this.OnGetApplicationSettings();
        }

        public string ReadConfigurationXml()
        {
            return this.OnReadConfigurationXml();
        }

        public ExceptionDetails GetExceptionDetails(Exception exception, ExceptionDetails parentExceptionDetails)
        {
            return this.OnGetExceptionDetails(exception, parentExceptionDetails);
        }

        public IDebugOutput GetDebugOutput()
        {
            return this.OnGetDebugOutput();
        }

        public Stream CreateCompressedStream(Stream stream)
        {
            return stream;
        }
    }
}
