using Microsoft.HockeyApp.Common;
using Microsoft.HockeyApp.Model;
using Microsoft.HockeyApp.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace Microsoft.HockeyApp.ViewModels
{
    public class FeedbackFlyoutVM : VMBase
    {

        private ILog logger = HockeyLogManager.GetLog(typeof(FeedbackFlyoutVM));

        public string FlyoutTitle { get { return LocalizedStrings.LocalizedResources.FeedbackPageTitle; } }

        private ObservableCollection<FeedbackThreadVM> _feedbackThreads = new ObservableCollection<FeedbackThreadVM>();
        public ObservableCollection<FeedbackThreadVM> FeedbackThreadList
        {
            get { return _feedbackThreads; }
        }


        public double MinWidth { get { return Window.Current.Bounds.Width * 0.95; } }

        private FeedbackThreadVM _selectedThread;
        public FeedbackThreadVM SelectedFeedbackThread
        {
            get { return _selectedThread; }
            set { 
                _selectedThread = value;
                NotifyOfPropertyChange("SelectedFeedbackThread");
            }
        }

        private bool _initialized = false;

        internal async Task InitializeIfNeededAsync()
        {
            if (!_initialized)
            {
                this.IsBusy = true;
                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    var threads = (await FeedbackManager.Current.LoadFeedbackThreadsAsync()).ToList();
                    this.FeedbackThreadList.Clear();
                    foreach (var thread in threads.ToList())
                    {
                        this.FeedbackThreadList.Add(thread);
                    }
                    this.SelectedFeedbackThread = this.FeedbackThreadList.First();
                    _initialized = true;
                }
                if (!_initialized)
                {
                    await new MessageDialog(LocalizedStrings.LocalizedResources.FeedbackNoInternet).ShowAsync();
                }
                this.IsBusy = false;
            }
        }

        public FeedbackFlyoutVM()
        {
            RefreshCommand = new RelayCommand(() =>
            {
                this.IsBusy = true;
                var tasks = FeedbackThreadList.Where(t => !t.FeedbackThread.IsNewThread)
                                .Select(t => FeedbackManager.Current.RefreshFeedbackThreadVMAsync(SelectedFeedbackThread));
                Task.WaitAll(tasks.ToArray(),20);
                this.IsBusy = false;
            });

            AddThreadCommand = new RelayCommand(() =>
            {
                var threadVM = new FeedbackThreadVM();
                this.FeedbackThreadList.Add(threadVM);
                FeedbackManager.Current.AddFeedbackThread(threadVM);
                this.SelectedFeedbackThread = threadVM;
            });

            CloseThreadCommand = new RelayCommand(async () =>
            {
                var msg = this.SelectedFeedbackThread.IsNewThread ? 
                    LocalizedStrings.LocalizedResources.CloseNewThreadQuestion
                    : LocalizedStrings.LocalizedResources.CloseActiveThreadQuestion;
                var dialog = new MessageDialog(msg);
                dialog.Commands.Add(new UICommand() { Id = true, Label = LocalizedStrings.LocalizedResources.Yes });
                dialog.Commands.Add(new UICommand() { Id = false, Label = LocalizedStrings.LocalizedResources.No });
                var result = await dialog.ShowAsync();
                FeedbackFlyoutVM.ShowFlyout();
                if ((bool)result.Id)
                {
                    if (this.SelectedFeedbackThread.IsNewThread) { FeedbackManager.Current.SaveFeedbackThreadTokens(); }
                    this.FeedbackThreadList.Remove(this.SelectedFeedbackThread);
                    if (!this.FeedbackThreadList.Any())
                    {
                        this.FeedbackThreadList.Add(new FeedbackThreadVM());
                    }
                    this.SelectedFeedbackThread = this.FeedbackThreadList.First();
                }
            });
        }

        #region Commands

        public ICommand AddThreadCommand { get; protected set; }
        public ICommand CloseThreadCommand { get; protected set; }
        public ICommand RefreshCommand { get; protected set; }


        #endregion

        protected static FeedbackFlyout CurrentFlyout {get; set;}

        internal static void ShowFlyout()
        {
            if (CurrentFlyout != null)
            {
                CurrentFlyout.ShowIndependent();
            }
        }

        internal void CalledFromNewFlyout(FeedbackFlyout feedbackFlyout)
        {
            CurrentFlyout = feedbackFlyout;
            if (!this.FeedbackThreadList.Any())
            {
                this.FeedbackThreadList.Add(new FeedbackThreadVM());
            }
            this.SelectedFeedbackThread = this.FeedbackThreadList.First();
        }
    }
}
