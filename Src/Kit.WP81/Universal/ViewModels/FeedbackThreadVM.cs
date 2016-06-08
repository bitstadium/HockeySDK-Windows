using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.HockeyApp.Tools;
using Windows.Storage;
using Microsoft.HockeyApp.Model;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.HockeyApp.Common;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.HockeyApp.Views;

namespace Microsoft.HockeyApp.ViewModels
{
    public partial class FeedbackThreadVM : VMBase
    {

        public FeedbackThreadVM(IFeedbackThread feedbackThread = null)
        {
            this.FeedbackThread = feedbackThread ?? HockeyApp.Model.FeedbackThread.CreateInstance();
            SetCommands();

        }

        

        #region properties

        public string Email
        {
            get
            {
                if (this.FeedbackThread.Messages != null && this.FeedbackThread.Messages.Count > 0)
                {

                    return this.FeedbackThread.Messages.First().Email;
                }
                return null;
            }
        }

        public string Username
        {
            get
            {
                if (this.FeedbackThread.Messages != null && this.FeedbackThread.Messages.Count > 0)
                {

                    return this.FeedbackThread.Messages.First().Name;
                }
                return null;
            }
        }

        private string _subject = null;
        public string Subject
        {
            get
            {
                if (this.FeedbackThread.Messages != null && this.FeedbackThread.Messages.Count > 0)
                {

                    return this.FeedbackThread.Messages.First().Subject;
                }
                return _subject;
            }
            set 
            {
                _subject = value;
                NotifyOfPropertyChange("Subject");

            }
        }

        private ObservableCollection<FeedbackMessageVM> _messages = new ObservableCollection<FeedbackMessageVM>();
        public ObservableCollection<FeedbackMessageVM> Messages
        {
            get { return _messages; }
        }

        
        public bool IsNewThread
        {
            get { return this.FeedbackThread.IsNewThread; }
        }

        #endregion

        private IFeedbackThread _fbThread;

        public IFeedbackThread FeedbackThread
        {
            get { return _fbThread; }
            set { 
                _fbThread = value;
                NotifyOfPropertyChange("FeedbackThread");
                this._messages.Clear();
                foreach(var msgVM in this.FeedbackThread.Messages.Select(msg => new FeedbackMessageVM(msg as FeedbackMessage))) {
                    this._messages.Add(msgVM);
                }
                NotifyOfPropertyChange("Messages");
            }
        }



        internal async Task<IFeedbackMessage> PostMessageAsync(FeedbackMessageVM msgVM)
        {
            var msg = await this.FeedbackThread.PostFeedbackMessageAsync(msgVM.Message, msgVM.Email, msgVM.Subject, msgVM.Username, msgVM.Attachments.Select(att => att.FeedbackAttachment));
            FeedbackManager.Current.SaveFeedbackThreadTokens();
            return msg;
        }

    }
}
