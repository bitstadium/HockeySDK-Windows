using System;
using System.Windows.Forms;
using System.Globalization;

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
            _applicationId = appId ?? Application.ProductName;
            _version = appVersion ?? Application.ProductVersion;
            _storeRegion = storeRegion ?? CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
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
