using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Microsoft.HockeyApp.Extensions
{
    // some place holders while I figure out if the PEReader/Native image debug info is accessible in a similar manner as the UWP implementaiton
    // TODO - repo case
    internal static class StackFrameExtension
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
            return IntPtr.Add(GetNativeImageBase(stackFrame), stackFrame.GetNativeOffset());
        }
    }
}