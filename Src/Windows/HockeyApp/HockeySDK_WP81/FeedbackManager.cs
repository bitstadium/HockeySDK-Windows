using HockeyApp.ViewModels;
using HockeyApp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Core;
using HockeyApp.Tools;
using Windows.UI.Xaml.Navigation;
using HockeyApp.Views;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Foundation;
using Windows.Graphics.Display;

namespace HockeyApp
{
    internal partial class FeedbackManager
    {
        internal const string FilePickerContinuationKey = "FilePickerHockeyAppReactivation";

        private FeedbackMessageVM _msgVM = new FeedbackMessageVM();
        internal FeedbackMessageVM CurrentFeedbackMessageVM
        {
            get { return _msgVM; }
            set { _msgVM = value; }
        }

        private FeedbackThreadVM _threadVM = new FeedbackThreadVM();
        internal FeedbackThreadVM CurrentFeedbackThreadVM 
        {
            get { return _threadVM; }
            set { _threadVM = value; }
        }

        private bool _feedbackUsedInCurrentSession = false;

        #region Storage

        #endregion

        internal async Task ClearMessageCacheAsync()
        {
            this.CurrentFeedbackMessageVM = new FeedbackMessageVM()
            {
                FeedbackThreadVM = this.CurrentFeedbackThreadVM
            };
            await FeedbackMessageVM.ClearAttachmentTmpDir();
        }

        internal async Task<FeedbackThreadVM> TryRestoreFeedbackThread()
        {
            try
            {
                await this.LoadFeedbackThreadsAsync();
                var threadVM = this.OpenFeedbackThreadVMs.FirstOrDefault();
                this.CurrentFeedbackThreadVM = threadVM; 
                return this.CurrentFeedbackThreadVM;
            }
            catch (Exception ex)
            {
                HockeyClient.Current.AsInternal().HandleInternalUnhandledException(ex);
                return null;
            }
        }

        internal void HandleSentMessage(IFeedbackMessage sentMsg)
        {
            if(!this.CurrentFeedbackThreadVM.Messages.Any(msgVM => msgVM.FeedbackMessage.Id == sentMsg.Id)){
                this.CurrentFeedbackThreadVM.Messages.Add(new FeedbackMessageVM(sentMsg as FeedbackMessage));
            }
        }

        protected async Task InitializeIfNeeded()
        {
            if (!_feedbackUsedInCurrentSession)
            {
                this.CurrentFeedbackMessageVM = await FeedbackMessageVM.LoadFeedbackMessageVMFromStorageAsync();
                await TryRestoreFeedbackThread();
                _feedbackUsedInCurrentSession = true;
            }
        }

        internal async Task StoreDataIfNeeded()
        {
            if (_feedbackUsedInCurrentSession)
            {
                await FeedbackMessageVM.ClearAttachmentTmpDir();
                await this.CurrentFeedbackMessageVM.SaveToStorageWithAttachments();
            }
        }

        #region PageTransitions

        internal async Task<object> GetDataContextNavigatedToImagePage(NavigationEventArgs args, Canvas canvas, Image showArea)
        {
            await InitializeIfNeeded();
            dynamic pars = args.Parameter as DynamicNavigationParameters ?? new DynamicNavigationParameters();

            FeedbackAttachmentVM attachVM = pars.Attachment; //when called from Formpage
            if (attachVM == null) // called from filePickerResume
            {
                attachVM = new FeedbackAttachmentVM();
                attachVM.IsNewAttachment = true;
                var file = (pars.ImageFile as StorageFile);
                attachVM.FeedbackAttachment.FileName = file.Name;
                var img = await file.GetBitmapImageAsync();
                attachVM.InitialImage = img;
            }

            attachVM.Canvas = canvas;

            if (attachVM.IsNotReadOnly)
            {
                canvas.Children.Clear();
                canvas.Background = attachVM.InitialImage.AsImageBrush();
            }
            
            attachVM.OrientationChanged(canvas, showArea);

            return attachVM;
        }

        internal async Task<object> GetDataContextNavigatedToListPage(NavigationEventArgs args, ListBox messageList)
        {
            await InitializeIfNeeded();
            dynamic pars = args.Parameter as DynamicNavigationParameters ?? new DynamicNavigationParameters();

            var frame = (Window.Current.Content as Frame);

            if (args.NavigationMode == NavigationMode.Back && CurrentFeedbackThreadVM.FeedbackThread.IsNewThread)
            {
                await CoreWindow.GetForCurrentThread().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    frame.GoBack();
                });
                return CurrentFeedbackThreadVM;
            }
            else
            {

                CurrentFeedbackThreadVM.Messages.CollectionChanged += async (s, o) => await messageList.ScrollToEnd();
                if (true == pars.IsCallFromApp) //called from HockeyClient-Extension
                {
                    CurrentFeedbackThreadVM.IsBusy = true;
                    var reloadedThread = await FeedbackManager.Current.TryRestoreFeedbackThread();
                    if (reloadedThread != null)
                    {
                        if (CurrentFeedbackThreadVM.FeedbackThread.IsNewThread)
                        {
                            await CoreWindow.GetForCurrentThread().Dispatcher.RunIdleAsync((o) => frame.Navigate(typeof(FeedbackFormPage)));
                        }
                        await messageList.ScrollToEnd();
                    }
                    else
                    {
                        await new MessageDialog(LocalizedStrings.LocalizedResources.FeedbackNoInternet).ShowAsync();
                        await CoreWindow.GetForCurrentThread().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            frame.GoBack();
                        });
                    }
                }
                return CurrentFeedbackThreadVM;
            }
        }

        internal async Task<object> GetDataContextNavigatedToFormPage(NavigationEventArgs args)
        {
            await InitializeIfNeeded();

            var attach = (args.Parameter as FeedbackAttachmentVM);
            if (args.NavigationMode != NavigationMode.Back && attach != null)
            {
                CurrentFeedbackMessageVM.HandleAttachment(attach);
            }
            return CurrentFeedbackMessageVM;

        }
        
        #endregion
    }
}
