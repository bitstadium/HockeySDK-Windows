using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml.Media.Imaging;

namespace Microsoft.HockeyApp.Tools
{

    public class GravatarHelper
    {
        private static Dictionary<string, BitmapImage> _gravatarCache = new Dictionary<string, BitmapImage>();

        public static string CreateHash(string str)
        {
            var alg = HashAlgorithmProvider.OpenAlgorithm("MD5");
            IBuffer buff = CryptographicBuffer.ConvertStringToBinary(str, BinaryStringEncoding.Utf8);
            var hashed = alg.HashData(buff);
            var res = CryptographicBuffer.EncodeToHexString(hashed);
            return res;
        }

        public static async Task<BitmapImage> LoadGravatar(string hash)
        {
            if (hash == null) { hash = ""; }

            if (_gravatarCache.ContainsKey(hash))
            {
                return _gravatarCache[hash];
            }

            string url = "http://s.gravatar.com/avatar/" + hash;

            BitmapImage bi;

            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                StreamContent sc = response.Content as StreamContent;
                bi = new BitmapImage();
                await bi.SetSourceAsync((await sc.ReadAsStreamAsync()).AsRandomAccessStream());

                //because of async loading of all gravatar infos...
                Monitor.Enter(_gravatarCache);
                if (!_gravatarCache.ContainsKey(hash))
                {
                    _gravatarCache.Add(hash, bi);
                }
                Monitor.Exit(_gravatarCache);
            }
            else
            {
                bi = GravatarHelper.DefaultGravatar;
            }

            return bi;
        }

        private static BitmapImage _defaultGravatar = null;
        public static BitmapImage DefaultGravatar
        {
            get
            {
                if (_defaultGravatar == null)
                {
                    _defaultGravatar = LocalizedAssets.LocalizedBitmapImage.DefaultGravatar as BitmapImage;
                }
                return _defaultGravatar;
            }
        }

    }
}
