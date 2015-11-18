using Microsoft.HockeyApp.Model;
using Microsoft.HockeyApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.HockeyApp
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
