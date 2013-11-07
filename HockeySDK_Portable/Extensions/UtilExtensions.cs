using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace HockeyApp.Extensions
{
    public static class UtilExtensions
    {
        /// <summary>
        /// Convert a unix epoch-timestamp to a DateTime
        /// </summary>
        /// <param name="unixTimeStamp">seconds since epoch (1.1.1970)</param>
        /// <returns></returns>
        public static DateTime? UnixTimeStampToDateTime(this Int64? unixTimeStamp)
        {
            if (unixTimeStamp == null || unixTimeStamp == 0)
            {
                return null;
            }
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = CultureInfo.CurrentCulture.Calendar.AddSeconds(dtDateTime, (int)unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        /// <summary>
        /// Convert a number of bytes to a short readable string
        /// </summary>
        /// <param name="byteCount">number of bytes</param>
        /// <returns>string representation (e.g. '3 mb')</returns>
        public static String ToReadableByteString(this Int64 byteCount)
        {
            string[] suf = { " byte", " kb", " mb", " gb", " tb", "pb", "eb" };
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString(CultureInfo.InvariantCulture) + suf[place];
        }
    }
}
