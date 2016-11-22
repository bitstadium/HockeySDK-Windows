namespace Microsoft.HockeyApp.Services.Device
{
    using System;
    using System.Runtime.InteropServices;

    internal static class NativeMethods
    {
        [DllImport("kernel32.dll")]
        internal static extern void GetNativeSystemInfo(ref _SYSTEM_INFO systemInfo);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:AccessibleFieldsMustBeginWithUpperCaseLetter", Justification = "_SYSTEM_INFO class specifics.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "_SYSTEM_INFO class specifics.")]
        [StructLayout(LayoutKind.Sequential)]
        internal struct _SYSTEM_INFO
        {
            public ushort wProcessorArchitecture;

            public ushort wReserved;

            public uint dwPageSize;

            public IntPtr lpMinimumApplicationAddress;

            public IntPtr lpMaximumApplicationAddress;

            public UIntPtr dwActiveProcessorMask;

            public uint dwNumberOfProcessors;

            public uint dwProcessorType;

            public uint dwAllocationGranularity;

            public ushort wProcessorLevel;

            public ushort wProcessorRevision;
        }
    }
}