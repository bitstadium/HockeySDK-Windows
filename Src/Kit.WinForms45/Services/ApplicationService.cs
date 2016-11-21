using System;

namespace Microsoft.HockeyApp.Services
{
    sealed class ApplicationService : IApplicationService
    {
#pragma warning disable 0067
        public event EventHandler OnResuming;
        public event EventHandler OnSuspending;
#pragma warning restore 0067

        private readonly string _storeRegion;
        private readonly string _version;
        private readonly string _applicationId;

        public ApplicationService(string appId, string appVersion, string storeRegion)
        {
            if (string.IsNullOrEmpty(appId)) { throw new ArgumentException("appId"); }
            if (string.IsNullOrEmpty(appVersion)) { throw new ArgumentException("appVersion"); }
            if (string.IsNullOrEmpty(storeRegion)) { throw new ArgumentException("storeRegion"); }

            _applicationId = appId;
            _version = appVersion;
            _storeRegion = storeRegion;
        }

        public string GetApplicationId() => _applicationId;

        public string GetStoreRegion() => _storeRegion;

        public string GetVersion() => _version;

        public void Init()
        {

        }

        public bool IsDevelopmentMode()
        {
            return false;
        }
    }
}
