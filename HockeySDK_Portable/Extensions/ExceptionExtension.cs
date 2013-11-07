using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HockeyApp.Extensions
{
    public static class ExceptionExtension
    {
        public static String StackTraceToString(this Exception @this)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(@this.GetType().ToString());
            builder.Append(": ");
            builder.Append(string.IsNullOrEmpty(@this.Message) ? "No reason" : @this.Message);
            builder.AppendLine();
            builder.Append(string.IsNullOrEmpty(@this.StackTrace) ? "  at unknown location" : @this.StackTrace);

            Exception inner = @this.InnerException;
            if ((inner != null) && (!string.IsNullOrEmpty(inner.StackTrace)))
            {
                builder.AppendLine();
                builder.AppendLine("Inner Exception");
                builder.Append(inner.StackTrace);
            }

            return builder.ToString().Trim();
        }
    }
}
