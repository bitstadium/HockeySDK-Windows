namespace Microsoft.HockeyApp.Extensibility
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using global::Windows.Globalization;

    /// <summary>
    /// The reader is platform specific and applies to WinRT applications only.
    /// </summary>
    public class UserContextReader
    {
        private static bool? isNativeEnvironment;

        /// <summary>
        /// Gets the store region.
        /// </summary>
        /// <returns>The two-letter identifier for the user's region.</returns>
        public static string GetStoreRegion()
        {
            var userRegion = new GeographicRegion();
            string regionCode = userRegion.CodeTwoLetter;

            return regionCode;
        }

        /// <summary>
        /// Determines whether the application is compiled with .NET Native tool chain
        /// </summary>
        /// <returns>True if compiled with .NET Native tool chain, otherwise false.</returns>
        public static bool IsNativeEnvironment(Exception exception)
        {
            if (!isNativeEnvironment.HasValue)
            { 
                var stackTrace = new StackTrace(exception, true);
                StackFrame[] stackFrames = stackTrace.GetFrames();
                if (stackFrames != null && stackFrames.Length > 0)
                {
                    var stackFrame = stackFrames[0];
                    IntPtr imageBase = stackFrame.GetNativeImageBase();

                    // imageBase is set to a value other than zero if application is compiled with .NET Native tool chain. 
                    isNativeEnvironment = imageBase != IntPtr.Zero;
                } else
                {
                    // we don't know, but as all applications in store will run in will be .NET Native compiled, set this value to true.
                    isNativeEnvironment = true;
                }

            }

            return isNativeEnvironment.Value;
        }
    }
}