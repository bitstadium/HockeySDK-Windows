using HockeyApp.Model;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;

namespace HockeyApp.Tools
{
    public static class UtilExtensions
    {

        public static Regex RegexForLikeMatching(this string @this, string globPattern)
        {
            string regexPattern = Regex.Escape(globPattern).
                Replace(@"\*", ".+?").
                Replace(@"\?", ".");
            return new Regex(regexPattern, RegexOptions.IgnoreCase);
        }


        public static bool IsValidEmail(this string str)
        {
            if (str == null) { return false; }
            const String pattern =
                @"^([0-9a-zA-Z]" + //Start with a digit or alphabate
                @"([\+\-_\.][0-9a-zA-Z]+)*" + // No continues or ending +-_. chars in email
                @")+" +
                @"@(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]*\.)+[a-zA-Z0-9]{2,17})$";

            return Regex.IsMatch(str, pattern);
        }

        public static bool IsEmpty(this string str)
        {
            return str == null || str.Trim().Length == 0;
        }

        public static bool SetValue(this IsolatedStorageSettings settings, string key, object value)
        {
            if (settings.Contains(key))
            {
                settings[key] = value;
                return false;
            }
            else
            {
                settings.Add(key, value);
                return true;
            }
        }

        public static bool RemoveValue(this IsolatedStorageSettings settings, string key)
        {
            if (settings.Contains(key))
            {
                settings.Remove(key);
                return true;
            }
            return false;
        }
        
        public static object GetValue(this IsolatedStorageSettings settings, string key)
        {
            if (settings.Contains(key))
            {
                return settings[key];
            }
            return null;
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
                    a = byte.Parse(hexColorString.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                    start = 2;
                }

                //convert RGB characters to bytes
                r = byte.Parse(hexColorString.Substring(start, 2), System.Globalization.NumberStyles.HexNumber);
                g = byte.Parse(hexColorString.Substring(start + 2, 2), System.Globalization.NumberStyles.HexNumber);
                b = byte.Parse(hexColorString.Substring(start + 4, 2), System.Globalization.NumberStyles.HexNumber);

                return Color.FromArgb(a, r, g, b);
            }
            catch (Exception)
            {
                return defaultColor;
            }
        }

    }
}
