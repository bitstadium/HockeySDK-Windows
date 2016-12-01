using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

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
            return Marshal.GetHINSTANCE(stackFrame.GetMethod().Module.Assembly.ManifestModule);
        }

        public static IntPtr GetNativeIP(this StackFrame stackFrame)
        {
            // Definitely wrong, but we need to return something
            // probably need something like this https://msdn.microsoft.com/en-us/library/dn832657(v=vs.110).aspx (only works on .net 4.6)
            return IntPtr.Add(GetNativeImageBase(stackFrame), stackFrame.GetNativeOffset());
        }
    }
}
