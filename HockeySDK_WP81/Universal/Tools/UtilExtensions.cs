using HockeyApp.Model;
using System;
using System.Collections.Generic;
using Windows.Storage;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;

namespace HockeyApp.Tools
{
    internal static class UtilExtensions
    {

        internal static void SetValue(this IPropertySet self, String key, String value)
        {
            self[key] = value;
        }

        internal static void RemoveValue(this IPropertySet self, String key)
        {
            if (self.ContainsKey(key))
            {
                self.Remove(key);
            }
        }

        internal static string GetValue(this IPropertySet self, String key)
        {
            if(self.ContainsKey(key))
            {
                return self[key] as String;
            }
            return null;
        }

        internal static Regex RegexForLikeMatching(this string @this, string globPattern)
        {
            string regexPattern = Regex.Escape(globPattern).
                Replace(@"\*", ".+?").
                Replace(@"\?", ".");
            return new Regex(regexPattern, RegexOptions.IgnoreCase);
        }

        internal static async Task<StorageFolder> GetLocalFolderByNameCreateIfNotExistingAsync(this string @this)
        {
            var localFolder = ApplicationData.Current.LocalFolder;
            StorageFolder folder = localFolder;
#if DEBUG
            if (@this.Contains('\\') || @this.Contains('/')) { throw new Exception("only flat directorynames (no /) supported!"); }
#endif
            if (!@this.IsEmpty())
            {
                folder = await localFolder.CreateFolderAsync(@this, CreationCollisionOption.OpenIfExists);
            }

            return folder;
        }


        internal static bool IsValidEmail(this string str)
        {
            if (str == null) { return false; }
            const String pattern =
                @"^([0-9a-zA-Z]" + //Start with a digit or alphabate
                @"([\+\-_\.][0-9a-zA-Z]+)*" + // No continues or ending +-_. chars in email
                @")+" +
                @"@(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]*\.)+[a-zA-Z0-9]{2,17})$";

            return Regex.IsMatch(str, pattern);
        }

        internal static bool IsEmpty(this string str)
        {
            return str == null || str.Trim().Length == 0;
        }

        internal static async Task EncodeWriteableBitmapAsync(this WriteableBitmap bmp, IRandomAccessStream writeStream, Guid encoderId)
        {
            // Copy buffer to pixels
            byte[] pixels;
            using (var stream = bmp.PixelBuffer.AsStream())
            {
                pixels = new byte[(uint)stream.Length];
                await stream.ReadAsync(pixels, 0, pixels.Length);
            }

            // Encode pixels into stream
            var encoder = await BitmapEncoder.CreateAsync(encoderId, writeStream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied,
               (uint)bmp.PixelWidth, (uint)bmp.PixelHeight,
               96, 96, pixels);
            await encoder.FlushAsync();
        }

    }
}
