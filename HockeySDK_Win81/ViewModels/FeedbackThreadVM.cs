using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HockeyApp.ViewModels
{
    public partial class FeedbackThreadVM
    {

        private FeedbackMessageVM _newMsgVM = new FeedbackMessageVM();
        public FeedbackMessageVM NewMessage
        {
            get { return _newMsgVM; }
            set { _newMsgVM = value; }
        }

        private void SetCommands()
        {
            _newMsgVM.FeedbackThreadVM = this;
        }

        #region commands


        #endregion

    }
}
