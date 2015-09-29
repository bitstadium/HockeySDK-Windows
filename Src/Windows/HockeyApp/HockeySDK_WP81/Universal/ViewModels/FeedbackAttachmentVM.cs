using HockeyApp.Model;
using System;
using System.Collections.Generic;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using HockeyApp.Common;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using HockeyApp.Views;
using HockeyApp.Tools;

namespace HockeyApp.ViewModels
{
    public partial class FeedbackAttachmentVM : VMBase
    {
        public FeedbackAttachmentVM(FeedbackAttachment attachment = null)
        {
            this.FeedbackAttachment = attachment ?? new FeedbackAttachment(null, null, "image/jpeg");

            SetCommands();
        }

        private FeedbackAttachment _feedbackAttachment;
        public FeedbackAttachment FeedbackAttachment
        {
            get { return _feedbackAttachment; }
            set
            {
                _feedbackAttachment = value;
                NotifyOfPropertyChange("FeedbackAttachment");
                NotifyOfPropertyChange("Label");
            }
        }

        #region Commands
        public ICommand ShowImageCommand { get; protected set; }

        #endregion

        #region Properties

        public BitmapImage InitialImage { get; set; }

        private IBuffer _imageBuffer = null;
        public IBuffer ImageBuffer
        {
            get
            {
                if (_imageBuffer == null && this.FeedbackAttachment.DataBytes != null)
                {
                    _imageBuffer = WindowsRuntimeBuffer.Create(this.FeedbackAttachment.DataBytes, 0, this.FeedbackAttachment.DataBytes.Length, this.FeedbackAttachment.DataBytes.Length);
                }
                return _imageBuffer;
            }
            set { _imageBuffer = value; }
        }

        public String Label
        {
            get { return this.FeedbackAttachment.FileName; }
        }

        public bool IsReadOnly
        {
            get { return !String.IsNullOrEmpty(this.FeedbackAttachment.RemoteURL); }
        }

        public Uri RemoteUrl
        {
            get
            {
                if (this.FeedbackAttachment == null || this.FeedbackAttachment.RemoteURL == null)
                {
                    return null;
                }
                else
                {
                    return new Uri(this.FeedbackAttachment.RemoteURL, UriKind.Absolute);
                }
            }
        }

        private bool _markedAsDeleted = false;
        public bool MarkedAsDeleted
        {
            get { return _markedAsDeleted; }
            set { _markedAsDeleted = value; }
        }

        private Boolean _isNew = true;
        public Boolean IsNewAttachment
        {
            get { return _isNew; }
            set { _isNew = value; }
        }
        #endregion


    }
}
