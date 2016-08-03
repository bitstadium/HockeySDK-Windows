using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.HockeyApp.Tools;

namespace Microsoft.HockeyApp
{
    public class LocalizedAssets
    {
        private static AssetWrapper _localizedAssets = null;

        public static dynamic LocalizedBitmapImage
        {
            get
            {
                if (_localizedAssets == null)
                {
                    _localizedAssets = new AssetWrapper();
                }
                return _localizedAssets;
            }
        }
    }

    public class AssetWrapper : DynamicObject
    {
        Dictionary<string,BitmapImage> customBitmapsAlreadyChecked = new Dictionary<string,BitmapImage>();

        public object this[string index]
        {
            get
            {
                BitmapImage alreadyLoadedImage = null;
                customBitmapsAlreadyChecked.TryGetValue(index, out alreadyLoadedImage);

                if (alreadyLoadedImage != null)
                {
                    return alreadyLoadedImage;
                }
                else
                {
                    var image = new BitmapImage();
                    ExceptionRoutedEventHandler failedHandler = null;
                    failedHandler = (s, e) =>
                    {
                        var img = s as BitmapImage;
                        img.ImageFailed -= failedHandler;
                        img.UriSource = new Uri("ms-appx:///" + WebBrowserHelper.AssemblyNameWithoutExtension + "/Assets/" + index + ".png");
                    };
                    image.ImageFailed += failedHandler;
                    var customUri = new Uri("ms-appx:///Assets/HockeyApp/" + index + ".png");
                    image.UriSource = customUri;
                    customBitmapsAlreadyChecked.Add(index, image);
                    return image;
                }
            }
        }

        //For dynamic use in code
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = this[binder.Name];
            return result != null;
        }
    }
}
