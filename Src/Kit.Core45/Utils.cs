namespace Microsoft.HockeyApp
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using Extensibility.Implementation.Tracing;

    /// <summary>
    /// Various utilities.
    /// </summary>
    internal static partial class Utils
    {
        public static bool IsNullOrWhiteSpace(this string value)
        {
            if (value == null)
            {
                return true;
            }

            return string.IsNullOrWhiteSpace(value);
        }

        public static void CopyDictionary<TValue>(IDictionary<string, TValue> source, IDictionary<string, TValue> target)
        {
            foreach (KeyValuePair<string, TValue> pair in source)
            {
                if (string.IsNullOrEmpty(pair.Key))
                {
                    continue;
                }

                if (!target.ContainsKey(pair.Key))
                {
                    target[pair.Key] = pair.Value;
                }
            }
        }

        /// <summary>
        /// Validates the string and if null or empty populates it with '$parameterName is a required field for $telemetryType' value.
        /// </summary>
        public static string PopulateRequiredStringValue(string value, string parameterName, string telemetryType)
        {
            if (string.IsNullOrEmpty(value))
            {
                CoreEventSource.Log.LogVerbose(string.Format(CultureInfo.InvariantCulture, "Value for property '{0}' of {1} was not found. Populating it by default.", parameterName, telemetryType));
                return parameterName + " is a required field for " + telemetryType;
            }

            return value;
        }

        /// <summary>
        /// Returns default Timespan value if not a valid Timespan.
        /// </summary>
        public static TimeSpan ValidateDuration(string value)
        {
            TimeSpan interval;
            if (!TimeSpanEx.TryParse(value, CultureInfo.InvariantCulture, out interval))
            {
                CoreEventSource.Log.LogError("Invalid duration for Request Telemetry. Setting it to '00:00:00'.");
                return TimeSpan.Zero;
            }

            return interval;
        }

        internal static bool EqualsWithPrecision(this double value1, double value2, double precision)
        {
            return (value1 >= value2 - precision) && (value1 <= value2 + precision);
        }
    }
}
