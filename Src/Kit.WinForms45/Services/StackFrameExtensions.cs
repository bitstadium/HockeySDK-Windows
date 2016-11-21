using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Microsoft.HockeyApp.Services
{
    // some place holders while I figure out if the PEReader/Native image debug info is accessible in a similar manner as the UWP implementaiton
    // TODO - repo case
    static class StackFrameExtensions
    {
        public static bool HasNativeImage(this StackFrame stackFrame)
        {
            return stackFrame.GetNativeImageBase() != IntPtr.Zero;
        }

        public static IntPtr GetNativeImageBase(this StackFrame stackFrame)
        {
            return IntPtr.Zero;
        }
        public static IntPtr GetNativeIP(this StackFrame stackFrame)
        {
            return IntPtr.Zero;
        }

    }
}
