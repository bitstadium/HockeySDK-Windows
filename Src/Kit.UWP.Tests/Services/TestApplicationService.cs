using System;

namespace Microsoft.HockeyApp.Services
{
    class TestApplicationService : IApplicationService
    {
        public event EventHandler OnResuming
        {
            add { }
            remove { }
        }

        public event EventHandler OnSuspending
        {
            add { }
            remove { }
        }

        public string GetApplicationId()
        {
            return new Guid().ToString();
        }

        public string GetStoreRegion()
        {
            return "en";
        }

        public string GetVersion()
        {
            return "0.0.0.0";
        }

        public void Init()
        {
        }

        public bool IsDevelopmentMode()
        {
            return true;
        }
    }
}
