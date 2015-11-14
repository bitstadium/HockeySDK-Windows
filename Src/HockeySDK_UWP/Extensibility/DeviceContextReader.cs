namespace Microsoft.HockeyApp.Extensibility
{
    using System;
    using System.Diagnostics;

    internal partial class DeviceContextReader
    {
        private static bool? isNativeEnvironment;

        /// <summary>
        /// Determines whether the application is compiled with .NET Native tool chain
        /// </summary>
        /// <returns>True if compiled with .NET Native tool chain, otherwise false.</returns>
        internal static bool IsNativeEnvironment(Exception exception)
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
                }
                else
                {
                    // we don't know, but as all applications in store will run in will be .NET Native compiled, set this value to true.
                    isNativeEnvironment = true;
                }
            }

            return isNativeEnvironment.Value;
        }

    }
}
