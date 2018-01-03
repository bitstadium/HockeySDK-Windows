﻿namespace Microsoft.HockeyApp.Services
{
    using System;
    using System.Globalization;
    using Windows.ApplicationModel;
    using Windows.ApplicationModel.Core;
#if WP8
    using Microsoft.Phone.Shell;
    using System.Reflection;
#endif

    internal class ApplicationService : IApplicationService
    {
        /// <summary>
        /// The default application version we will be returning if no application version is found.
        /// </summary>
        internal const string UnknownComponentVersion = "Unknown";

        /// <summary>
        /// The version for this component.
        /// </summary>
        private string version;

        private string fullPackageName;

        private bool initialized = false;

        public event EventHandler OnResuming;

        public event EventHandler OnSuspending;

        public void Init()
        {
            if (initialized)
            {
                return;
            }

#if WP8
            PhoneApplicationService.Current.Activated += Current_Activated;
            //PhoneApplicationService.Current.Launching += Current_Launching;

            PhoneApplicationService.Current.Deactivated += Current_Deactivated;
            PhoneApplicationService.Current.Closing += Current_Closing;
#else
            CoreApplication.Resuming += CoreApplication_Resuming;
            CoreApplication.Suspending += CoreApplication_Suspending;
#endif
            initialized = true;
        }

#if WP8
        private void Current_Activated(object sender, ActivatedEventArgs e)
        {
            if (OnResuming != null)
            {
                this.OnResuming(this, null);
            }
        }

        private void Current_Launching(object sender, LaunchingEventArgs e)
        {
            if (OnResuming != null)
            {
                this.OnResuming(this, null);
            }
        }

        private void Current_Deactivated(object sender, DeactivatedEventArgs e)
        {
            if (OnSuspending != null)
            {
                this.OnSuspending(this, null);
            }
        }


        private void Current_Closing(object sender, ClosingEventArgs e)
        {
            if (OnSuspending != null)
            {
                this.OnSuspending(this, null);
            }
        }
#else
        private void CoreApplication_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            if (OnSuspending != null)
            {
                this.OnSuspending(this, null);
            }
        }

        internal void CoreApplication_Resuming(object sender, object e)
        {
            if (OnResuming != null)
            {
                this.OnResuming(this, null);
            }
        }
#endif

        public bool IsDevelopmentMode()
        {
            // IsDevelopmentMode API is supported only in UWP, for all others return false.
#if WINDOWS_UWP
            return Windows.ApplicationModel.Package.Current.IsDevelopmentMode;
#else
            return false;
#endif
        }

        /// <summary>
        /// Gets the version for the current application. If the version cannot be found, we will return the passed in default.
        /// </summary>
        /// <returns>The extracted data.</returns>
        public string GetVersion()
        {
            if (this.version != null)
            {
                return this.version;
            }

            string temp = null;
#if WP8
            try
            {
                var currentPackage = Package.Current;
                // NotImplementedException is threw on Windows Phone Silverlight 8.0 app when accessing Id property
                // Windows Phone Silverlight 8.1 app is OK.
                if (currentPackage != null && currentPackage.Id != null)
                {
                    temp = string.Format(
                                        CultureInfo.InvariantCulture,
                                        "{0}.{1}.{2}.{3}",
                                        currentPackage.Id.Version.Major,
                                        currentPackage.Id.Version.Minor,
                                        currentPackage.Id.Version.Build,
                                        currentPackage.Id.Version.Revision);
                }
            }
            catch (NotImplementedException)
            {
                // Use maniest file version
                temp = ManifestHelper.GetAppVersion();
                if (string.IsNullOrWhiteSpace(temp))
                {
                    // Use assembly version
                    var versionAttribute = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyFileVersionAttribute), true).GetValue(0) as AssemblyFileVersionAttribute;
                    temp = versionAttribute.Version.ToString();
                }
            }
#else
            var currentPackage = Package.Current;
            if (currentPackage != null && currentPackage.Id != null)
            {
                temp = string.Format(
                                    CultureInfo.InvariantCulture,
                                    "{0}.{1}.{2}.{3}",
                                    currentPackage.Id.Version.Major,
                                    currentPackage.Id.Version.Minor,
                                    currentPackage.Id.Version.Build,
                                    currentPackage.Id.Version.Revision);
            }
#endif // WP8
            if (string.IsNullOrEmpty(temp) == false)
            {
                return this.version = temp;
            }

            return this.version = UnknownComponentVersion;
        }

        /// <summary>
        /// Gets the application identifier, which is the namespace name for App class.
        /// </summary>
        /// <returns>Namespace name for App class.</returns>
        public string GetApplicationId()
        {
            if (this.fullPackageName == null)
            {
#if WP8
                this.fullPackageName = System.Windows.Application.Current.GetType().Namespace;
#else
                this.fullPackageName =  global::Windows.UI.Xaml.Application.Current.GetType().Namespace;
#endif
            }

            return this.fullPackageName;
        }

        /// <summary>
        /// Gets the store region.
        /// </summary>
        /// <returns>The two-letter identifier for the user's region.</returns>
        public string GetStoreRegion()
        {
#if WP8
            return string.Empty;
#else
            return new global::Windows.Globalization.GeographicRegion().CodeTwoLetter;
#endif
        }
    }
}
