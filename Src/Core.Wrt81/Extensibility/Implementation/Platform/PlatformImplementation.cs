//namespace Microsoft.HockeyApp.Extensibility.Implementation.Platform
//{
//    using System;
//    using System.Collections.Generic;
//    using System.IO;
//    using System.Linq;
//    using Extensibility;
//    using External;
//    using Implementation;
//    using global::Windows.ApplicationModel;
//    using global::Windows.Storage;

//    /// <summary>
//    /// Windows Runtime (Phone and Store) implementation of the <see cref="IPlatform"/> interface.
//    /// </summary>
//    internal class PlatformImplementation : IPlatform
//    {
//        public PlatformImplementation()
//        {
//        }

//        public IDictionary<string, object> GetLocalApplicationSettings()
//        {
//            return ApplicationData.Current.LocalSettings.Values;
//        }

//        public IDictionary<string, object> GetRoamingApplicationSettings()
//        {
//            return ApplicationData.Current.RoamingSettings.Values;
//        }

//        public string ReadConfigurationXml()
//        {
//            StorageFile file = Package.Current.InstalledLocation
//                .GetFilesAsync().GetAwaiter().GetResult()
//                .FirstOrDefault(f => f.Name == "HockeyApp.config");

//            if (file != null)
//            {
//                Stream stream = file.OpenStreamForReadAsync().GetAwaiter().GetResult();
//                using (StreamReader reader = new StreamReader(stream))
//                {
//                    return reader.ReadToEnd();
//                }
//            }

//            return string.Empty;
//        }

//        public ExceptionDetails GetExceptionDetails(Exception exception, ExceptionDetails parentExceptionDetails)
//        {
//            return ExceptionConverter.ConvertToExceptionDetails(exception, parentExceptionDetails);
//        }

//        public IDebugOutput GetDebugOutput()
//        {
//            return new DebugOutput();
//        }
//    }
//}
