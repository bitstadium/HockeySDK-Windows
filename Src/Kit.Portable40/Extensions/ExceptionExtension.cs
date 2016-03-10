using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.HockeyApp.Extensions
{
    internal class NamespaceDoc { }

    /// <summary>
    /// static extension class for extensions on exception
    /// </summary>
    internal static class ExceptionExtension
    {
        /// <summary>
        /// build a formatted string from the stacktrace of an exception
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        internal static String StackTraceToString(this Exception @this)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(@this.GetType().ToString() + ": ");
            builder.Append(string.IsNullOrEmpty(@this.Message) ? "No reason" : @this.Message);
            builder.AppendLine();
            builder.AppendLine(string.IsNullOrEmpty(@this.StackTrace) ? "  at unknown location" : @this.StackTrace);

            if (@this is AggregateException)
            {
                HandleAggregateException(@this as AggregateException, builder);
            }
            else
            {
                HandleInnerExceptionHierarchy(@this.InnerException, builder);
            }

            return builder.ToString().Trim();
        }

        private static void HandleInnerExceptionHierarchy(Exception inner, StringBuilder builder)
        {
            if ((inner != null) && (!string.IsNullOrEmpty(inner.StackTrace)))
            {
                builder.AppendLine("Caused by: " + inner.GetType().ToString() + ": " + inner.Message);
                builder.AppendLine(inner.StackTrace);

                HandleInnerExceptionHierarchy(inner.InnerException, builder);
            }
        }

        private static void HandleAggregateException(AggregateException ex, StringBuilder builder)
        {
            int i = 0;
            foreach (Exception inner in ex.InnerExceptions)
            {
                builder.AppendLine();
                builder.AppendLine("Aggregated Exception [" + i++ + "]: " + inner.GetType().ToString() + ": " + inner.Message);
                builder.AppendLine(string.IsNullOrEmpty(inner.StackTrace) ? "  at unknown location" : inner.StackTrace);
                HandleInnerExceptionHierarchy(inner.InnerException, builder);
            }
        }

    }
}
