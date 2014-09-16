using HockeyApp.Common;
using HockeyApp.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media;

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

        protected async Task LoadGravatar(string hash)
        {
            this.Gravatar = await GravatarHelper.LoadGravatar(hash);
        }

        private async Task AfterSendActionAsync(IFeedbackMessage sentMessage)
        {
            
        }

        public ICommand AddAttachmentCommand { get; internal set; }

        public ICommand RemoveAttachmentCommand { get; internal set; }

        public ICommand SendMessageCommand { get; internal set; }

        public ICommand CancelMessageCommand { get; internal set; }
        

        private void SetCommands()
        {
            AddAttachmentCommand = new RelayCommand(async () =>
            {

                var picker = new FileOpenPicker();
                picker.ViewMode = PickerViewMode.List;
                var files = await picker.PickMultipleFilesAsync();
                foreach (var file in files)
                {
                    this.Attachments.Add(new FeedbackAttachmentVM());
                }

            });

            RemoveAttachmentCommand = new RelayCommand(async (o) => { 
                await new MessageDialog("Remove" + o.ToString()).ShowAsync(); 
            });

            SendMessageCommand = new RelayCommand(async (o) => { 
                await new MessageDialog("SEnd").ShowAsync(); 
            });

            CancelMessageCommand = new RelayCommand(async (o) => { 
                await new MessageDialog("Cancel").ShowAsync(); 
            });

            if (this.FeedbackMessage != null && !String.IsNullOrWhiteSpace(this.FeedbackMessage.GravatarHash))
            {
                Task t = this.LoadGravatar(this.FeedbackMessage.GravatarHash);
            }
            else
            {
                this.Gravatar = GravatarHelper.DefaultGravatar;
            }

        }
    }
}
