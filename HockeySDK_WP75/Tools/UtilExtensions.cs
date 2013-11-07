using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HockeyApp.Tools
{
    public static class UtilExtensions
    {

        public static DateTime? UnixTimeStampToDateTime(this Int64? unixTimeStamp)
        {
            if (unixTimeStamp == null || unixTimeStamp == 0)
            {
                return null;
            }
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = new GregorianCalendar().AddSeconds(dtDateTime, (int) unixTimeStamp).ToLocalTime();
            return dtDateTime;
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

        public static String ToReadableByteString(this Int64 byteCount)
        {
            string[] suf = { " byte", " kb", " mb", " gb", " tb", "pb", "eb" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString(CultureInfo.InvariantCulture) + suf[place];
        }

    }
}
