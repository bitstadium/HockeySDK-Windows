using HockeyApp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.System;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;
using System.IO;
using Windows.Storage;

namespace HockeyApp.ViewModels
{
    public partial class FeedbackAttachmentVM
    {
        public void SetCommands()
        {

            RemoveAttachmentCommand = new RelayCommand(() =>
            {
                if (this.FeedbackMessageVM != null)
                {
                    this.FeedbackMessageVM.Attachments.Remove(this);
                }
            });

            OpenAttachmentCommand = new RelayCommand(async () =>
            {
                await OpenAttachmentAsync();
            });

        }

        internal async Task OpenAttachmentAsync() {
            if (!String.IsNullOrEmpty(this.FeedbackAttachment.RemoteURL))
            {
                await Launcher.LaunchUriAsync(new Uri(this.FeedbackAttachment.RemoteURL));
            }
            else
            {
                var folder = ApplicationData.Current.TemporaryFolder;
                var file = await folder.CreateFileAsync(DateTime.Now.Ticks.ToString() + this.FeedbackAttachment.FileName ,CreationCollisionOption.ReplaceExisting);
                
                using (var stream = await file.OpenStreamForWriteAsync()) {
                    await this.FeedbackAttachment.DataBytes.AsBuffer().AsStream().CopyToAsync(stream);
                }
                await Launcher.LaunchFileAsync(file);
            }
        }

        internal FeedbackMessageVM FeedbackMessageVM { get; set; }

        public ICommand RemoveAttachmentCommand { get; internal set; }
        public ICommand OpenAttachmentCommand { get; internal set; }


    }
}
