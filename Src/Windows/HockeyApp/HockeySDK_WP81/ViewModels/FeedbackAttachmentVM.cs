using HockeyApp.Common;
using HockeyApp.Tools;
using HockeyApp.Views;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml.Media.Imaging;

namespace HockeyApp.ViewModels
{
    public partial class FeedbackAttachmentVM
    {

        private void SetCommands()
        {
            this.ShowImageCommand = new RelayCommand(() =>
            {
                dynamic pars = new DynamicNavigationParameters();
                pars.Attachment = this;
                (Window.Current.Content as Frame).Navigate(typeof(FeedbackImagePage), pars);
            });

            SaveImageCommand = new RelayCommand(async () =>
            {
                ImageBuffer = await Canvas.SaveAsPngIntoBufferAsync();
                InitialImage = await ImageBuffer.AsBitmapImageAsync();
                (Window.Current.Content as Frame).Navigate(typeof(FeedbackFormPage), this);
            });
            DeleteImageCommand = new RelayCommand(() =>
            {
                MarkedAsDeleted = true;
                (Window.Current.Content as Frame).Navigate(typeof(FeedbackFormPage), this);
            });
            ResetImageCommand = new RelayCommand(() =>
            {
                Canvas.Children.Clear();
                Canvas.Background = InitialImage.AsImageBrush();
            });
            CloseImageCommand = new RelayCommand(() => { (Window.Current.Content as Frame).GoBack(); });

        }

        public bool IsNotReadOnly { get { return !IsReadOnly; } }

        internal async void OrientationChanged(Canvas canvas, Image image)
        {
            if (this.IsReadOnly)
            {
                image.Height = ScreenResolution.HeightWithoutScale;
            }
            else
            {
                this.IsBusy = true;
                BitmapImage attachImage = this.InitialImage;
                double width = attachImage.PixelWidth, height = attachImage.PixelHeight;
                if (Math.Max(attachImage.PixelWidth, attachImage.PixelHeight) > ScreenResolution.HeightWithoutScale)
                {
                    if (attachImage.PixelWidth > attachImage.PixelHeight)
                    {
                        width = ScreenResolution.HeightWithoutScale;
                        height = ScreenResolution.HeightWithoutScale * ((double)attachImage.PixelHeight / attachImage.PixelWidth);
                    }
                    else
                    {
                        height = ScreenResolution.HeightWithoutScale;
                        width = ScreenResolution.HeightWithoutScale * ((double)attachImage.PixelWidth / attachImage.PixelHeight);
                    }
                }
                if (canvas.Children.Any())
                {
                    this.ImageBuffer = await canvas.SaveAsPngIntoBufferAsync();
                    InitialImage = await ImageBuffer.AsBitmapImageAsync();
                    canvas.Background = this.InitialImage.AsImageBrush();
                    canvas.Children.Clear();
                }
                canvas.Width = width;
                canvas.Height = height;
                this.IsBusy = false;
            }
        }

        internal Canvas Canvas { get; set; }

        #region Commands

        public ICommand DeleteImageCommand { get; private set; }
        public ICommand SaveImageCommand { get; private set; }
        public ICommand ResetImageCommand { get; private set; }
        public ICommand CloseImageCommand { get; private set; }

        #endregion

        #region Storage

        internal const string AttachmentFileExt = ".attach";
        internal const string AttachmentNameSeparator = "__";

        internal static async Task<FeedbackAttachmentVM> LoadFromStorageAsync(string filename)
        {
            var helper = HockeyClient.Current.AsInternal().PlatformHelper;
            var imageFileName = filename.Substring(filename.IndexOf(AttachmentNameSeparator) + 1).Replace(AttachmentFileExt, "");

            var fbAttachVM = new FeedbackAttachmentVM();

            using (var stream = (await helper.GetStreamAsync(filename, ConstantsUniversal.FeedbackAttachmentTmpDir)).AsRandomAccessStream())
            {
                var buffer = WindowsRuntimeBuffer.Create((int)stream.Size);
                await stream.ReadAsync(buffer, (uint)stream.Size, InputStreamOptions.None);
                fbAttachVM.ImageBuffer = buffer;
                fbAttachVM.InitialImage = await buffer.AsBitmapImageAsync();
                fbAttachVM.FeedbackAttachment.FileName = imageFileName; 
            }
            return fbAttachVM;
        }

        internal async Task SaveToStorageAsync(int counter)
        {
            var helper = HockeyClient.Current.AsInternal().PlatformHelper;
            await helper.WriteStreamToFileAsync(this.ImageBuffer.AsStream(), 
                counter + AttachmentNameSeparator + this.FeedbackAttachment.FileName + AttachmentFileExt , ConstantsUniversal.FeedbackAttachmentTmpDir);
        }

        #endregion
    }
}
