namespace Microsoft.HockeyApp.Services
{
    using Microsoft.HockeyApp.Extensibility;
    using Microsoft.HockeyApp.Extensibility.Implementation;
    using Microsoft.HockeyApp.Extensibility.Implementation.External;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using Windows.ApplicationModel;
    using Windows.Storage;

    class PlatformService : IPlatformService
    {
        public IDictionary<string, object> GetLocalApplicationSettings()
        {
            return ApplicationData.Current.LocalSettings.Values;
        }

        public IDictionary<string, object> GetRoamingApplicationSettings()
        {
            return ApplicationData.Current.RoamingSettings.Values;
        }

        public string ReadConfigurationXml()
        {
            StorageFile file = Package.Current.InstalledLocation
                .GetFilesAsync().GetAwaiter().GetResult()
                .FirstOrDefault(f => f.Name == "HockeyApp.config");

            if (file != null)
            {
                Stream stream = file.OpenStreamForReadAsync().GetAwaiter().GetResult();
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }

            return string.Empty;
        }

        public ExceptionDetails GetExceptionDetails(Exception exception, ExceptionDetails parentExceptionDetails)
        {
            return ExceptionConverter.ConvertToExceptionDetails(exception, parentExceptionDetails);
        }

        public IDebugOutput GetDebugOutput()
        {
            return new DebugOutput();
        }

        public Stream CreateCompressedStream(Stream stream)
        {
            return new GZipStream(stream, CompressionMode.Compress);
        }
    }
}
