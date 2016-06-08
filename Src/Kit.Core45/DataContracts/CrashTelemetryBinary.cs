namespace Microsoft.HockeyApp.DataContracts
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Microsoft.HockeyApp.Channel;
    using Microsoft.HockeyApp.Extensibility.Implementation;
    using Microsoft.HockeyApp.Extensibility.Implementation.External;

    /// <summary>
    /// The type used to specify binary properties of a crash.
    /// </summary>
    internal sealed class CrashTelemetryBinary
    {
        internal readonly CrashDataBinary Data;

        /// <summary>
        /// Initializes a new instance of the <see cref="CrashTelemetryBinary"/> class.
        /// </summary>
        public CrashTelemetryBinary()
            : this(new CrashDataBinary())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CrashTelemetryBinary" /> class.
        /// </summary>
        /// <param name="binary">The binary.</param>
        internal CrashTelemetryBinary(CrashDataBinary binary)
        {
            this.Data = binary ?? new CrashDataBinary();
        }

        /// <summary>
        /// Gets or sets the start load memory address for this binary.
        /// </summary>
        public string StartAddress
        {
            get { return this.Data.startAddress; }
            set { this.Data.startAddress = value; }
        }

        /// <summary>
        /// Gets or sets the end load memory address for this binary.
        /// </summary>
        public string EndAddress
        {
            get { return this.Data.endAddress; }
            set { this.Data.endAddress = value; }
        }

        /// <summary>
        /// Gets or sets the name for this binary.
        /// </summary>
        public string Name
        {
            get { return this.Data.name; }
            set { this.Data.name = value; }
        }

        /// <summary>
        /// Gets or sets the CPU type that this binary was built for.
        /// </summary>
        public long CpuType
        {
            get { return this.Data.cpuType; }
            set { this.Data.cpuType = value; }
        }

        /// <summary>
        /// Gets or sets the CPU sub-type that this binary was built for.
        /// </summary>
        public long CpuSubType
        {
            get { return (ushort)this.Data.cpuSubType; }
            set { this.Data.cpuSubType = value; }
        }

        /// <summary>
        /// Gets or sets the unique identifier for this binary.
        /// </summary>
        public string Uuid
        {
            get { return this.Data.uuid; }
            set { this.Data.uuid = value; }
        }

        /// <summary>
        /// Gets or sets the load path for this binary.
        /// </summary>
        public string Path
        {
            get { return this.Data.path; }
            set { this.Data.path = value; }
        }

        /// <summary>
        /// Sanitizes the properties based on constraints.
        /// </summary>
        internal void Sanitize()
        {
        }
    }
}
