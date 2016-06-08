using Microsoft.HockeyApp.Common;
using Microsoft.HockeyApp.Model;
using Microsoft.HockeyApp.Tools;
using Microsoft.HockeyApp.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Networking.Connectivity;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Microsoft.HockeyApp.ViewModels
{
    public partial class FeedbackMessageVM
    {
        protected Type FeedbackImagePageType
        {
            get { return typeof(FeedbackImagePage); }
        }


        private void SetCommands()
        {
            // Window.Current can be null in case of application is suspended.
            // issue discussed at https://support.hockeyapp.net/discussions/problems/57803-hockeysdkwinrt-version-223-bug
            Frame rootFrame = Window.Current == null ?  null : (Window.Current.Content as Frame);

            CancelCommand = new RelayCommand(() =>
            {
                if (rootFrame != null)
                {
                    rootFrame.GoBack();
                }
            });

            SendCommand = new RelayCommand(async () =>
            {
                bool success = true;
                if (await ValidateInputAsync())
                {
                    if(NetworkInterface.GetIsNetworkAvailable())
                    {
                    this.IsBusy = true;
                    try
                    {
                        IFeedbackMessage sentMessage = await this.SendFeedbackAsync();
                        await FeedbackManager.Current.ClearMessageCacheAsync();
                        await AfterSendActionAsync(sentMessage);
                    }
                    catch (WebException e)
                    {
                        HockeyClient.Current.AsInternal().HandleInternalUnhandledException(e);
                        success = false;
                    }
                    finally
                    {
                        this.IsBusy = false;
                    }
                    if (!success)
                    {
                        await new MessageDialog(LocalizedStrings.LocalizedResources.FeedbackSendError).ShowAsync();
                    }
                    } 
                    else
                    {
                        await new MessageDialog(LocalizedStrings.LocalizedResources.FeedbackNoInternet).ShowAsync();
                    }
                }
            });

            AttachImgCommand = new RelayCommand(() =>
            {
                PickPhoto();
            });

            EditAttachmentCommand = new RelayCommand((attachVM) =>
            {
                var vm = attachVM as FeedbackAttachmentVM;
                vm.IsNewAttachment = false;
                dynamic pars = new DynamicNavigationParameters();
                pars.Attachment = vm;
                if (rootFrame != null)
                {
                    rootFrame.Navigate(this.FeedbackImagePageType, pars);
                }
            });

        }

        public bool IsEmailReadOnly
        {
            get { return IsThreadActive && !String.IsNullOrWhiteSpace(this.Email); }
        }

        #region Serialization

        internal static async Task ClearAttachmentTmpDir()
        {
            var helper = HockeyClient.Current.AsInternal().PlatformHelper;
            var files = (await helper.GetFileNamesAsync(ConstantsUniversal.FeedbackAttachmentTmpDir)).ToList();
            foreach (string file in files)
            {
                await helper.DeleteFileAsync(file, ConstantsUniversal.FeedbackAttachmentTmpDir).ConfigureAwait(false);
            }
        }

        internal async Task SaveToStorageWithAttachments()
        {
            int i = 0;
            foreach (var attach in this.Attachments.Where(a => a.ImageBuffer != null))
            {
                await attach.SaveToStorageAsync(i++);
            }
            await this.FeedbackMessage.SaveToStorageAsync().ConfigureAwait(false);
        }


        internal static async Task<FeedbackMessageVM> LoadFeedbackMessageVMFromStorageAsync()
        {
            var helper = HockeyClient.Current.AsInternal().PlatformHelper;
            var feedbackVM = new FeedbackMessageVM();
            if (await helper.FileExistsAsync(ConstantsUniversal.OpenFeedbackMessageFile, ConstantsUniversal.FeedbackAttachmentTmpDir))
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(FeedbackMessage));
                using (var stream = await helper.GetStreamAsync(ConstantsUniversal.OpenFeedbackMessageFile, ConstantsUniversal.FeedbackAttachmentTmpDir))
                {
                    feedbackVM.FeedbackMessage = serializer.ReadObject(stream) as FeedbackMessage;
                }
                var files = await helper.GetFileNamesAsync(ConstantsUniversal.FeedbackAttachmentTmpDir,"*" + FeedbackAttachmentVM.AttachmentFileExt);
                foreach(var filename in files.ToList().OrderBy(f => f))
                {
                    feedbackVM.Attachments.Add(await FeedbackAttachmentVM.LoadFromStorageAsync(filename));
                }
            }
            return feedbackVM;
        }

        #endregion

        private async Task AfterSendActionAsync(IFeedbackMessage sentMsg)
        {
            FeedbackManager.Current.HandleSentMessage(sentMsg);
            var frame = (Window.Current.Content as Frame);
            frame.Navigate(typeof(FeedbackMainPage));
            await CoreWindow.GetForCurrentThread().Dispatcher.RunIdleAsync((o) =>
            {
                //removeme from backstack
                var pageStackEntry = frame.BackStack.LastOrDefault(entry => entry.SourcePageType == typeof(FeedbackFormPage));
                if (pageStackEntry != null) { frame.BackStack.Remove(pageStackEntry); }
            });
        }

        public Thickness Margin
        {
            get
            {
                return IsIncoming ? new Thickness(2, 4.5, 40, 4.5)
                    : new Thickness(40, 4.5, 2, 4.5);
            }
        }

        protected void PickPhoto()
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.ContinuationData.Add(FeedbackManager.FilePickerContinuationKey, "HockeyApp");
            openPicker.PickSingleFileAndContinue();
        }
    }
}
