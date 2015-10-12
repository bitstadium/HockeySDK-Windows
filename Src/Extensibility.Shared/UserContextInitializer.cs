namespace Microsoft.HockeyApp.Extensibility.Windows
{
    using System;
    using System.Collections.Generic;
    using Channel;
    using DataContracts;
    using Extensibility.Implementation;
    using Extensibility.Implementation.Platform;

    /// <summary>
    /// Tracks anonymous user Id for Store Apps (Windows Store and Windows Phone).
    /// </summary>
    public sealed class UserContextInitializer : ITelemetryInitializer
    {
        private const string UserIdSetting = "ApplicationInsightsUserId";
        private const string UserAcquisitionDateSetting = "ApplicationInsightsUserAcquisitionDate";
        private static object syncRoot = new object();

        private string userId;
        private DateTimeOffset? userAcquisitionDate;

        /// <summary>
        /// Initializes <see cref="UserContext.Id"/> property of the <see cref="TelemetryContext.User"/> telemetry
        /// and updates the IsFirst property of the SessionContext.
        /// </summary>
        public void Initialize(ITelemetry telemetry)
        {
            this.InitializeUserData();
            telemetry.Context.User.Id = this.userId;
            telemetry.Context.User.AcquisitionDate = this.userAcquisitionDate;
            telemetry.Context.User.StoreRegion = UserContextReader.GetStoreRegion();
        }

        private void InitializeUserData()
        {
            if (this.userId != null)
            {
                return;
            }

            lock (syncRoot)
            {
                if (this.userId != null)
                {
                    return;
                }

                IDictionary<string, object> settings = PlatformSingleton.Current.GetApplicationSettings();

                object storedUserAcquisitionDate;
                if (settings.TryGetValue(UserAcquisitionDateSetting, out storedUserAcquisitionDate))
                {
                    this.userAcquisitionDate = (DateTimeOffset)storedUserAcquisitionDate;
                }
                else
                {
                    this.userAcquisitionDate = DateTimeOffset.UtcNow;
                    settings[UserAcquisitionDateSetting] = this.userAcquisitionDate;
                }

                object storedUserId;
                if (settings.TryGetValue(UserIdSetting, out storedUserId))
                {
                    this.userId = (string)storedUserId;
                    return;
                }

                this.userId = Guid.NewGuid().ToString();
                settings[UserIdSetting] = this.userId;
            }
        }
    }
}
