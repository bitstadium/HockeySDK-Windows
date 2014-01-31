using HockeyApp.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HockeyApp.ViewModels
{
    public class FeedbackImageVM :VMBase
    {

        public FeedbackPageVM ParentVM { get; set; }

        public FeedbackImageVM(FeedbackPageVM parentVM)
        {
            this.ParentVM = parentVM;
            EditCommand = new DelegateCommand((obj) => { this.ParentVM.SwitchToImageEditor(this); });
        }

        #region Properties 

        private bool isEditable;
        public bool IsEditable
        {
            get { return isEditable; }
            set { 
                isEditable = value; 
            }
        }

        private IFeedbackImage fbImage;
        public IFeedbackImage FeedbackImage
        {
            get { return fbImage; }
            set { fbImage = value; }
        }
        
        public string Label
        {
            get { return this.FeedbackImage.FileName; }
        }

        public Uri RemoteUrl
        {
            get { return new Uri(this.FeedbackImage.RemoteURL, UriKind.Absolute); }
        }

        #endregion

        public DelegateCommand EditCommand { get; private set; }

        internal void SaveChangesToImage(InkPresenter ImageArea)
        {
            if (this.IsEditable)
            {
                WriteableBitmap wbBitmap = new WriteableBitmap(ImageArea, new TranslateTransform());
                using (var stream = new MemoryStream())
                {
                    wbBitmap.WritePNG(stream);
                    stream.Seek(0, System.IO.SeekOrigin.Begin);
                    byte[] imageBytes = new byte[stream.Length];
                    stream.Read(imageBytes, 0, (int)stream.Length);
                    this.FeedbackImage.ImageBytes = imageBytes;
                }
                ImageArea.Strokes.Clear();
            }

        }
    }
}
