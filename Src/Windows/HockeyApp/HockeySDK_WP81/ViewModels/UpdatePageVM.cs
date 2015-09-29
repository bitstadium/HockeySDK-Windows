using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HockeyApp.ViewModels
{
    public class UpdatePageVM : VMBase
    {

        private IAppVersion _newestVersion;

        public IAppVersion NewestVersion
        {
            get { return _newestVersion; }
            set { 
                _newestVersion = value;
                NotifyOfPropertyChange("NewestVersion");
            }
        }


    }
}
