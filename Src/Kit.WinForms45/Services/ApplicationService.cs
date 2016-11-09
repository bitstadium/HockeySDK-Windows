using System;

namespace Microsoft.HockeyApp.Services
{
    sealed class ApplicationService : IApplicationService
    {
        public event EventHandler OnResuming;
        public event EventHandler OnSuspending;

        private bool initialized = false;

        private readonly string _storeRegion;
        private readonly string _version;
        private readonly string _applicationId;

        public ApplicationService(string appId, string appVersion, string storeRegion)
        {
            _applicationId = appId;
            _version = appVersion;
            _storeRegion = storeRegion;
        }

        public string GetApplicationId() => _applicationId;

        public string GetStoreRegion() => _storeRegion;

        public string GetVersion() => _version;

        public void Init()
        {
            if (initialized)
            {
                return;
            }
            initialized = true;
        }

        public bool IsDevelopmentMode()
        {
            return false;
        }
    }
}
