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

        public static void PutInfo(this IsolatedStorageSettings settings, string key, string value)
        {
            if (settings.Contains(key))
            {
                settings[key] = value;
            }
            else
            {
                settings.Add(key, value);
            }
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
