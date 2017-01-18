namespace Microsoft.HockeyApp.Extensibility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides access to the processor architecture
    /// </summary>
    internal static class NativeMethods
    {
        /// <summary>
        /// Gets the native system information.
        /// </summary>
        /// <param name="lpSystemInfo">The lp system information.</param>
        [DllImport("kernel32.dll")]
        internal static extern void GetNativeSystemInfo(ref _SYSTEM_INFO lpSystemInfo);

        /// <summary>
        /// The system information struct
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct _SYSTEM_INFO
        {
            /// <summary>
            /// The processor architecture
            /// </summary>
            public short wProcessorArchitecture;

            /// <summary>
            /// The reserved
            /// </summary>
            public short wReserved;

            /// <summary>
            /// The page size
            /// </summary>
            public int dwPageSize;

            /// <summary>
            /// The minimum application address
            /// </summary>
            public IntPtr lpMinimumApplicationAddress;

            /// <summary>
            /// The maximum application address
            /// </summary>
            public IntPtr lpMaximumApplicationAddress;

            /// <summary>
            /// The active processor mask
            /// </summary>
            public IntPtr dwActiveProcessorMask;

            /// <summary>
            /// The number of processors
            /// </summary>
            public int dwNumberOfProcessors;

            /// <summary>
            /// The processor type
            /// </summary>
            public int dwProcessorType;

            /// <summary>
            /// The allocation granularity
            /// </summary>
            public int dwAllocationGranularity;

            /// <summary>
            /// The processor level
            /// </summary>
            public short wProcessorLevel;

            /// <summary>
            /// The processor revision
            /// </summary>
            public short wProcessorRevision;
        }
    }
}
