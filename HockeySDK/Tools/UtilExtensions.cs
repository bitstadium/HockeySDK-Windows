using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
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
