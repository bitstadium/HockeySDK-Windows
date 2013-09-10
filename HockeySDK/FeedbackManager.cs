using HockeyApp.Tools;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HockeyApp
{
    public class FeedbackManager
    {

        private static readonly FeedbackManager instance = new FeedbackManager();
        private string identifier = null;

        static FeedbackManager() { }
        private FeedbackManager() { }

        public static FeedbackManager Instance
        {
            get
            {
                return instance;
            }
        }


        public string AppIdentitfier { get { return identifier; } }

        public static void NavigateToFeedbackUI(string appIdentifier)
        {
            Instance.identifier = appIdentifier;
            FeedbackPopupTool.ShowPopup(new FeedbackVM(appIdentifier));
        }

    }
}
