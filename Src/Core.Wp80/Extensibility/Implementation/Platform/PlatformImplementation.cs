namespace Microsoft.HockeyApp.Extensibility.Implementation.Platform
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Windows;
    using System.Windows.Resources;
    using Channel;
    using Extensibility;
    using Extensibility.Implementation;
    using Extensibility.Implementation.External;

    /// <summary>
    /// The Windows Phone 8.0 (Silverlight Runtime) implementation of the <see cref="IPlatform" /> interface.
    /// </summary>
    internal class PlatformImplementation : 
        IPlatform
    {
        public PlatformImplementation()
        {
        }

        public IDictionary<string, object> GetApplicationSettings()
        {
            return IsolatedStorageSettings.ApplicationSettings;
        }        

        public string ReadConfigurationXml()
        {
            StreamResourceInfo streamInfo = Application.GetResourceStream(new Uri("HockeyApp.config", UriKind.Relative));
            if (streamInfo != null)
            {
                using (StreamReader reader = new StreamReader(streamInfo.Stream))
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
    }
}
