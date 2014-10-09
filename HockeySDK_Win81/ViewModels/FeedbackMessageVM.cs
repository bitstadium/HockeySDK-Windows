using HockeyApp.Common;
using HockeyApp.Model;
using HockeyApp.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage;
using System.IO;
using Windows.Storage.Streams;
using Windows.UI.ViewManagement;
using Windows.System;

namespace HockeyApp.ViewModels
{
    public partial class FeedbackMessageVM
    {

        private ImageSource _gravatar = null;
        public ImageSource Gravatar
        {
            get
            {
                return this._gravatar;
            }
            protected set
            {
                this._gravatar = value;
                NotifyOfPropertyChange("Gravatar");
            }
        }

        internal async Task ReLoadGravatar()
        {
            if(!String.IsNullOrEmpty(this.Email)) {
                this.FeedbackMessage.GravatarHash = GravatarHelper.CreateHash(this.Email);
                this.Gravatar = await GravatarHelper.LoadGravatar(this.FeedbackMessage.GravatarHash);
            }
        }

        public ICommand AddAttachmentCommand { get; internal set; }
        public ICommand OpenAttachmentCommand { get; internal set; }

        public ICommand SendMessageCommand { get; internal set; }
        public ICommand CancelMessageCommand { get; internal set; }

        private void SetCommands()
        {
            AddAttachmentCommand = new RelayCommand(async () =>
            {

                var picker = new FileOpenPicker();

                picker.ViewMode = PickerViewMode.List;
                picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                picker.FileTypeFilter.Add("*");

                var files = await picker.PickMultipleFilesAsync();
                foreach (var file in files)
                {
                    byte[] bytes = null;
                    using (var stream = await file.OpenReadAsync()) {
                        IBuffer buffer = WindowsRuntimeBuffer.Create((int)stream.Size);
                        await stream.ReadAsync(buffer, (uint)stream.Size, InputStreamOptions.None);
                        bytes = buffer.ToArray();
                    }
                    var attach = new FeedbackAttachment(file.Name, bytes, file.ContentType);
                    this.Attachments.Add(new FeedbackAttachmentVM(attach) { FeedbackMessageVM = this });
                }

            });

            OpenAttachmentCommand = new RelayCommand(async (o) =>
            {
                await ((FeedbackAttachmentVM)o).OpenAttachmentAsync();
            });

            SendMessageCommand = new RelayCommand(async (o) =>
            {

                bool success = true;
                if (await ValidateInputAsync())
                {
                    if (NetworkInterface.GetIsNetworkAvailable())
                    {
                        this.IsBusy = true;
                        try
                        {
                            IFeedbackMessage sentMessage = await this.SendFeedbackAsync();
                            this.FeedbackThreadVM.HandleSentMessage(sentMessage);
                        }
                        catch (Exception e)
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
                            FeedbackFlyoutVM.ShowFlyout();
                        }
                    }
                    else
                    {
                        await new MessageDialog(LocalizedStrings.LocalizedResources.FeedbackNoInternet).ShowAsync();
                        FeedbackFlyoutVM.ShowFlyout();
                    }

                }
                else
                {
                    FeedbackFlyoutVM.ShowFlyout();
                }
            });

            CancelMessageCommand = new RelayCommand((o) => {
                this.Message = null;
                this.Attachments.Clear();
            });

            if (!String.IsNullOrWhiteSpace(this.Email))
            {
                Task t = this.ReLoadGravatar();
            }
            else
            {
                this.Gravatar = GravatarHelper.DefaultGravatar;
            }

        }
    }
}
