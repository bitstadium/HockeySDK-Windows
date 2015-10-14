using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using System.Globalization;


namespace Microsoft.HockeyApp.Tools
{
    /// <summary>
    /// static class for utility extension methods
    /// </summary>
    public static class UtilWPExtensions
    {
        /// <summary>
        /// Save canvas contents as ImageData into buffer
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="dpiForImage"></param>
        /// <returns></returns>
        public static async Task<IBuffer> SaveAsPngIntoBufferAsync(this Canvas canvas, int dpiForImage = 200) {

            return await canvas.SaveAsPngIntoBufferAsync(DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel, dpiForImage).ConfigureAwait(false);
        }

        /// <summary>
        /// Converts a hex color string to a System.Windows.Media.Color
        /// </summary>
        /// <param name="hexColorString">a hex color string like #FFFFFF00</param>
        /// <param name="defaultColor">defautl value to if something goes wrong</param>
        /// <returns></returns>
        public static Color ConvertStringToColor(this String hexColorString, Color defaultColor)
        {
            try
            {
                //remove the # at the front
                hexColorString = hexColorString.Replace("#", "");

                byte a = 255;
                byte r = 255;
                byte g = 255;
                byte b = 255;

                int start = 0;

                //handle ARGB strings (8 characters long)
                if (hexColorString.Length == 8)
                {
                    a = byte.Parse(hexColorString.Substring(0, 2), System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    start = 2;
                }

                //convert RGB characters to bytes
                r = byte.Parse(hexColorString.Substring(start, 2), System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                g = byte.Parse(hexColorString.Substring(start + 2, 2), System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                b = byte.Parse(hexColorString.Substring(start + 4, 2), System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture);

                return Color.FromArgb(a, r, g, b);
            }
            catch (Exception)
            {
                return defaultColor;
            }
        }

    }
}
