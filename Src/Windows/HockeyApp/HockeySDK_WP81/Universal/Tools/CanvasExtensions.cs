using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace HockeyApp.Tools
{
    internal static class CanvasExtensions
    {

        public static async Task<BitmapImage> AsBitmapImageAsync(this IBuffer @this)
        {
            var img = new BitmapImage();
            using (var stream = new InMemoryRandomAccessStream())
            {
                await stream.WriteAsync(@this);
                stream.Seek(0);
                await img.SetSourceAsync(stream);
            }
            return img;
        }

        public static async Task<BitmapImage> GetBitmapImageAsync(this StorageFile @this)
        {
            BitmapImage bitmapImage = new BitmapImage();
            using (var stream = await @this.OpenAsync(FileAccessMode.Read)){
                bitmapImage.SetSource(stream);
            }
            return bitmapImage;

        }


        public static ImageBrush AsImageBrush(this ImageSource @this)
        {
            return new ImageBrush() { ImageSource = @this, Stretch = Stretch.Uniform };
        }

        public static async Task<IBuffer> SaveAsPngIntoBufferAsync(this Canvas canvas, double _scaleFactor, int dpiForImage = 200)
        {
            //string currentresolution = Window.Current.Bounds.Width * scaleFactor + "*" + Window.Current.Bounds.Height * scaleFactor;
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(canvas);
            var pixels = await renderTargetBitmap.GetPixelsAsync();
            using (IRandomAccessStream stream = new InMemoryRandomAccessStream())
            {
                var encoder = await
                    BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);

                byte[] bytes = pixels.ToArray();

                await CoreWindow.GetForCurrentThread().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    encoder.SetPixelData(BitmapPixelFormat.Bgra8,
                                         BitmapAlphaMode.Ignore,
                                         (uint)(canvas.ActualWidth * _scaleFactor), (uint)(canvas.ActualHeight * _scaleFactor),
                                         dpiForImage, dpiForImage, bytes);
                });

                await encoder.FlushAsync();
                stream.Seek(0);
                var buffer = WindowsRuntimeBuffer.Create((int)stream.Size);
                await stream.ReadAsync(buffer, (uint)stream.Size, InputStreamOptions.None);
                return buffer;
            }
        }
    }
}
