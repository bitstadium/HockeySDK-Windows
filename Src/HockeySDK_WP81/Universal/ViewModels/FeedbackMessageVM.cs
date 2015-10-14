using Microsoft.HockeyApp.Common;
using Microsoft.HockeyApp.Model;
using Microsoft.HockeyApp.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Graphics.Imaging;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using System.IO;
using System.Runtime.Serialization.Json;
using Windows.UI.Core;

namespace Microsoft.HockeyApp.ViewModels
{
    public partial class FeedbackMessageVM : VMBase
    {
        private ILog logger = HockeyLogManager.GetLog(typeof(FeedbackMessageVM));

        private FeedbackMessage _feedbackMessage;
        public FeedbackMessage FeedbackMessage
        {
            get { return _feedbackMessage; }
            set { _feedbackMessage = value; }
        }

        public FeedbackMessageVM(FeedbackMessage msg = null)
        {
            this._feedbackMessage = msg ?? new FeedbackMessage();
            SetCommands();
        }


        private FeedbackThreadVM _myThreadVM;
        public FeedbackThreadVM FeedbackThreadVM
        {
            get {
#if WINDOWS_PHONE_APP
                if (_myThreadVM == null)
                {
                    return FeedbackManager.Current.CurrentFeedbackThreadVM;
                }
#endif
                return _myThreadVM; }
            set { _myThreadVM = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal async Task<IFeedbackMessage> SendFeedbackAsync()
        {
            await FeedbackManager.Current.RefreshFeedbackThreadVMAsync(this.FeedbackThreadVM);

#if WINDOWS_PHONE_APP
            foreach (var attachment in attachments)
            {
                //convert all images to jpg for filesize
                attachment.FeedbackAttachment.DataBytes = await ConvertImageBufferToJpegBytes(attachment.ImageBuffer);
                attachment.FeedbackAttachment.FileName = Path.GetFileNameWithoutExtension(attachment.FeedbackAttachment.FileName) + ".jpg";
                attachment.FeedbackAttachment.ContentType = "image/jpeg";
            }
#endif

            IFeedbackMessage msg;
            msg = await this.FeedbackThreadVM.PostMessageAsync(this);
            return msg;
            
        }


        protected async Task<byte[]> ConvertImageBufferToJpegBytes(IBuffer imageBuffer)
        {
            using (var stream = imageBuffer.AsStream().AsRandomAccessStream())
            {
                var decoder = await BitmapDecoder.CreateAsync(stream);
                var pixels = await decoder.GetPixelDataAsync();
                using (var output = new InMemoryRandomAccessStream())
                {
                    var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, output);
                    await CoreWindow.GetForCurrentThread().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        encoder.SetPixelData(decoder.BitmapPixelFormat, BitmapAlphaMode.Ignore,
                            decoder.OrientedPixelWidth, decoder.OrientedPixelHeight, decoder.DpiX, decoder.DpiY, pixels.DetachPixelData());
                        
                    });
                    await encoder.FlushAsync();
                    var buffer = WindowsRuntimeBuffer.Create((int)output.Size);                    
                    output.Seek(0);
                    await output.ReadAsync(buffer, (uint)output.Size, InputStreamOptions.None);
                    return buffer.ToArray();
                }
            }
        }

        protected async Task<bool> ValidateInputAsync()
        {
            var errors = new List<string>();
            if (!this.Email.IsValidEmail())
            {
                errors.Add(LocalizedStrings.LocalizedResources.EmailEmptyWarning);
            }

            if (this.Subject.IsEmpty())
            {
                errors.Add(LocalizedStrings.LocalizedResources.SubjectEmptyWarning);
            }

            if (this.Username.IsEmpty())
            {
                errors.Add(LocalizedStrings.LocalizedResources.UserEmptyWarning);
            }

            if (this.Message.IsEmpty() && !this.Attachments.Any())
            {
                errors.Add(LocalizedStrings.LocalizedResources.MessageEmptyWarning);
            }

            if (errors.Any())
            {
                await new MessageDialog(errors.Aggregate((a, b) => a + "\n" + b)).ShowAsync();
                return false;
            }
            return true;
        }

        #region properties

        public string Via { get { return "via " + this.FeedbackMessage.ViaAsString; } }

        public string Email
        {
            get { return  this.FeedbackMessage.Email 
                    ?? ((this.FeedbackThreadVM != null) ? this.FeedbackThreadVM.Email : null) 
                    ?? FeedbackManager.Current.InitialEmail; }
            set
            {
#if WINDOWS_PHONE_APP
#else
                if (!String.IsNullOrEmpty(value) && value != this.FeedbackMessage.Email)
                {
                    Task t = ReLoadGravatar();
                }
#endif
                this.FeedbackMessage.Email = value;
                NotifyOfPropertyChange("Email");
            }
        }

        public string Subject
        {
            get { return this.FeedbackMessage.Subject ?? this.FeedbackThreadVM.Subject; }
            set
            {
                this.FeedbackMessage.Subject = value;
                NotifyOfPropertyChange("Subject");
            }
        }

        public string Message
        {
            get { return this.FeedbackMessage.CleanText ?? this.FeedbackMessage.Text; }
            set
            {
                this.FeedbackMessage.Text = value;
                NotifyOfPropertyChange("Message");
            }
        }

        public string Username
        {
            get { return this.FeedbackMessage.Name ?? this.FeedbackThreadVM.Username ?? FeedbackManager.Current.InitialUsername; }
            set
            {
                this.FeedbackMessage.Name = value;
                NotifyOfPropertyChange("Username");
            }
        }

        public bool IsThreadActive
        {
            get { return this.FeedbackThreadVM.Messages.Any(); }
        }

        public bool IsThreadNotActive
        {
            get { return !this.IsThreadActive; }
        }


        public bool IsIncoming { 
            get { return !IsOutgoing; } 
        }
        public bool IsOutgoing { 
            get { return this.FeedbackMessage.Via.Equals((int)FeedbackMessage.ViaTypes.API); } 
        }


        public string Created
        {
            get { return this.FeedbackMessage.Created.ToString(LocalizedStrings.LocalizedResources.FeedbackDateFormat); }
        }

        public string Text
        {
            get { return this.FeedbackMessage.CleanText; }
        }

        private ObservableCollection<FeedbackAttachmentVM> attachments = null;
        public ObservableCollection<FeedbackAttachmentVM> Attachments
        {
            get {
                if (attachments == null)
                {
                    this.attachments = new ObservableCollection<FeedbackAttachmentVM>(
                            this.FeedbackMessage.Attachments.Select(a => new FeedbackAttachmentVM(a as FeedbackAttachment)));
                }
                return attachments; }
        }
        internal void HandleAttachment(FeedbackAttachmentVM attachment)
        {
            if (attachment.MarkedAsDeleted)
            {
                if(!attachment.IsNewAttachment) {
                    this.Attachments.Remove(attachment);
                }
            }
            else if (attachment.IsNewAttachment)  {
                this.Attachments.Add(attachment);
            }
        }

        #endregion

        

        #region Commands

        public ICommand CancelCommand{ get; set; }

        public ICommand AttachImgCommand{ get; set; }

        public ICommand SendCommand{ get; set; }

        public ICommand EditAttachmentCommand { get; set; }

        #endregion
    }
}
