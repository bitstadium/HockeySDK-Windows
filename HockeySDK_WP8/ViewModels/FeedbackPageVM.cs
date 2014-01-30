using HockeyApp.Model;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Reactive;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using HockeyApp.Views;
using HockeyApp.Tools;

namespace HockeyApp.ViewModels
{
    public class FeedbackPageVM : VMBase
    {
        public Task Initialization { get; private set; }
        Action<FeedbackViewState> switchViewStateAction;

        public FeedbackPageVM(Action<FeedbackViewState> switchViewStateAction)
        {
            this.ThreadInfo = FeedbackManager.Instance.FeedbackPageTopTitle;
            this.switchViewStateAction = switchViewStateAction;
            Initialization = InitializeAsync(); //await this.Initialization to make shure its inititalized
        }

        private async Task InitializeAsync()
        {
            if (FeedbackManager.Instance.IsThreadOpen)
            {
                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    try
                    {
                        IFeedbackThread thread = await FeedbackManager.Instance.GetActiveThreadAsync(forceReload: true);
                        if (thread != null)
                        {
                            foreach (var msg in (thread.Messages))
                            {
                                this.Messages.Add(new FeedbackMessageVM(msg));
                            }
                            if (FeedbackManager.Instance.FeedbackPageTopTitle.IsEmpty() && !thread.Messages.First().Subject.IsEmpty())
                            {
                                Deployment.Current.Dispatcher.BeginInvoke(() => this.ThreadInfo = thread.Messages.First().Subject);
                            }
                        }
                        else //thread has been deleted
                        {
                            Deployment.Current.Dispatcher.BeginInvoke(() => IsThreadActive = false);
                            SwitchToMessageForm();
                        }
                    }
                    catch (Exception)
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            MessageBox.Show("There has been a connection problem with the server. Please try again later.");
                        });
                    }
                }
                else //no internet connection
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show("No Internet connection available. Please try again later.");
                    });
                }
            }
            else
            {
                Deployment.Current.Dispatcher.BeginInvoke(() => IsThreadActive = false);
                SwitchToMessageForm();
            }
            HideOverlay();
        }

        #region Properties

        private string threadinfo;
        public string ThreadInfo
        {
            get { return threadinfo; }
            set
            {
                threadinfo = value;
                NotifyOfPropertyChange("ThreadInfo");
            }
        }

        private bool isShowOverlay = true;
        public bool IsShowOverlay
        {
            get { return isShowOverlay; }
            set { 
                isShowOverlay = value;
                NotifyOfPropertyChange("IsShowOverlay");
            }
        }

        private bool isThreadActive = true;
        public bool IsThreadActive
        {
            get { return isThreadActive; }
            set
            {
                isThreadActive = value;
                NotifyOfPropertyChange("IsThreadActive");
            }
        }
        #endregion

        ObservableCollection<FeedbackMessageVM> messages = new ObservableCollection<FeedbackMessageVM>();
        public ObservableCollection<FeedbackMessageVM> Messages
        {
            get { return messages; }
        }

        public void SwitchToMessageList()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                this.IsShowOverlay = false;
            });
            switchViewStateAction(FeedbackViewState.MessageList);
        }

        public void SwitchToMessageForm()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => {
                this.IsShowOverlay = false;
            });
            switchViewStateAction(FeedbackViewState.MessageForm);
        }

        public void ShowOverlay()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => { this.IsShowOverlay = true; });
        }

        public void HideOverlay()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => { this.IsShowOverlay = false; });
        }

        private FeedbackImageVM iVM;
        public FeedbackImageVM CurrentImageVM
        {
            get { return iVM; }
            set { iVM = value; }
        }
        
        internal void SwitchToImageEditor(FeedbackImageVM imageVM)
        {
            CurrentImageVM = imageVM;
            if (imageVM.IsEditable)
            {
                switchViewStateAction(FeedbackViewState.ImageEdit);
            }
            else
            {
                switchViewStateAction(FeedbackViewState.ImageShow);
            }
        }

    }
}
