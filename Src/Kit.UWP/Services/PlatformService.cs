namespace Microsoft.HockeyApp.Services
{
    using Microsoft.HockeyApp.Extensibility;
    using Microsoft.HockeyApp.Extensibility.Implementation;
    using Microsoft.HockeyApp.Extensibility.Implementation.External;
    using System;
    using System.Collections.Generic;
    using System.IO;
#if WP8
    using System.IO.IsolatedStorage;
#endif
    using System.Linq;
    using Windows.ApplicationModel;
    using Windows.Storage;

    class PlatformService : IPlatformService
    {
#if WP8
        static volatile bool _isWP80 = false;
#endif

        public IDictionary<string, object> GetLocalApplicationSettings()
        {
#if WP8
            IDictionary<string, object> settings = null;
            if (!_isWP80)
            {
                try
                {
                    // NotImplementedException is threw on Windows Phone Silverlight 8.0 app
                    // Windows Phone Silverlight 8.1 app is OK.
                    settings = ApplicationData.Current.LocalSettings.Values;
                }
                catch (NotImplementedException)
                {
                    _isWP80 = true;
                }
            }
            if(_isWP80 && settings == null)
            {
                // for  Windows Phone Silverlight 8.0 app
                settings = IsolatedStorageSettings.ApplicationSettings;
            }
            return settings;
#else
            return ApplicationData.Current.LocalSettings.Values;
#endif
        }

        public IDictionary<string, object> GetRoamingApplicationSettings()
        {
#if WP8
            IDictionary<string, object> settings = null;
            if (!_isWP80)
            {
                try
                {
                    // NotImplementedException is threw on Windows Phone Silverlight 8.0 app
                    // Windows Phone Silverlight 8.1 app is OK.
                    settings = ApplicationData.Current.RoamingSettings.Values;
                }
                catch (NotImplementedException)
                {
                    _isWP80 = true;
                }
            }
            if (_isWP80 && settings == null)
            {
                // for  Windows Phone Silverlight 8.0 app
                // Use localSettings because Windows Phone Silverlight 8.0 app does not support roaming settings
                settings = IsolatedStorageSettings.ApplicationSettings;
            }
            return settings;
#else
            return ApplicationData.Current.RoamingSettings.Values;
#endif
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

        public string SdkName()
        {
#if WINDOWS_UWP
            return "HockeySDK.UWP";   
#elif WP8
            return "HockeySDK.WP";
#else
            return "HockeySDK.WINRT";
#endif
        }
    }
}
