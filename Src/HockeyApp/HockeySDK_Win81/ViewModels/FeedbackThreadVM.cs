using HockeyApp.Model;
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
            set { 
                _newMsgVM = value;
                NotifyOfPropertyChange("NewMessage");
            }
        }

        internal void HandleSentMessage(IFeedbackMessage sentMsg)
        {
            if (!this.Messages.Any(msgVM => msgVM.FeedbackMessage.Id == sentMsg.Id))
            {
                this.Messages.Add(new FeedbackMessageVM(sentMsg as FeedbackMessage));
            }
            this.NewMessage = new FeedbackMessageVM() { FeedbackThreadVM = this };
        }

        private void SetCommands()
        {
            _newMsgVM.FeedbackThreadVM = this;
            Task t = _newMsgVM.ReLoadGravatar();
        }

        public bool IsThreadActive
        {
            get { return this.Messages.Any(); }
        }

        public bool IsThreadNotActive
        {
            get { return !this.IsThreadActive; }
        }


        #region commands


        #endregion

    }
}
