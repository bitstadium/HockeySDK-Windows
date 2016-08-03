using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.Storage;
using Windows.UI.Xaml;

namespace Microsoft.HockeyApp.Tools
{
    internal static class WebBrowserHelper
    {

        private static string cssStyles = null;
        internal static async Task<string> GetCssStylesAsync()
        {
            if (cssStyles != null) { return cssStyles; }

            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///"+ WebBrowserHelper.AssemblyNameWithoutExtension +"/Assets/wp8releasenotes.css"));
            using (Stream stream = await file.OpenStreamForReadAsync())
            {
                cssStyles = new StreamReader(stream).ReadToEnd();
            }
            return cssStyles;
        }

        internal static async Task<string> WrapContentAsync(string content)
        {
            string theme = Application.Current.RequestedTheme.Equals(ApplicationTheme.Light) ? "light" : "dark";
            var builder = new StringBuilder();
            builder.Append("<html><head><meta name='viewport' content='width=device-width;user-scalable=no' /><style type='text/css'>");
            builder.Append(await WebBrowserHelper.GetCssStylesAsync());
            builder.Append("</style></head><body class=" + theme + " >");
            builder.Append(content);
            builder.Append("</body></html>");
            return builder.ToString();
        }

        public static string AssemblyNameWithoutExtension
        {
            get
            {
                return "Microsoft.HockeyApp.Kit";
            }
        }
    }
}
