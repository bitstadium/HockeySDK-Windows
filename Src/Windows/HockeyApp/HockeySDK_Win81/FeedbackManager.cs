using HockeyApp.Model;
using HockeyApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HockeyApp
{
    internal partial class FeedbackManager
    {

        private FeedbackFlyoutVM _currentFBVM = new FeedbackFlyoutVM();
        public FeedbackFlyoutVM CurrentFeedbackFlyoutVM
        {
            get { return _currentFBVM; }
            set { _currentFBVM = value; }
        }

    }
}
