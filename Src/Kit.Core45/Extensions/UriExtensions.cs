 // ReSharper disable once CheckNamespace
namespace System
{
    using System.Text;

    internal static class UriExtensions
    {
        public static string EscapeDataString(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            const int limit = 32760;

            string result;
            int length = value.Length;
            if (length < limit)
            {
                result = Uri.EscapeDataString(value);
            }
            else
            {
                int offset = 0;
                StringBuilder sb = new StringBuilder(length);
                while (offset < length)
                {
                    int size = Math.Min(length - offset, limit);
                    sb.Append(Uri.EscapeDataString(value.Substring(offset, size)));
                    offset += size;
                }
                result = sb.ToString();
            }
            return result;
        }
    }
}