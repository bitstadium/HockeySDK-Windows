namespace Microsoft.HockeyApp.DataContracts
{
    using System.Collections.Generic;
    using Microsoft.HockeyApp.Extensibility.Implementation.External;

    /// <summary>
    /// The type used to specify frame properties of a crash.
    /// </summary>
    internal sealed class CrashTelemetryThreadFrame
    {
        internal readonly CrashDataThreadFrame Data;
 
        /// <summary>
        /// Initializes a new instance of the <see cref="CrashTelemetryThreadFrame"/> class.
        /// </summary>
        public CrashTelemetryThreadFrame()
            : this(new CrashDataThreadFrame())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CrashTelemetryThreadFrame"/> class.
        /// </summary>
        internal CrashTelemetryThreadFrame(CrashDataThreadFrame headers)
        {
            this.Data = headers ?? new CrashDataThreadFrame();
        }

        /// <summary>
        /// Gets or sets the memory address for this frame.
        /// </summary>
        public string Address
        {
            get { return this.Data.address; }
            set { this.Data.address = value; }
        }

        /// <summary>
        /// Gets or sets the symbol (if any) for this frame.
        /// </summary>
        public string Symbol
        {
            get { return this.Data.symbol; }
            set { this.Data.symbol = value; }
        }

        /// <summary>
        /// Gets the set of registers we want to record for this frame.
        /// </summary>
        public IDictionary<string, string> Registers 
        {
            get { return this.Data.registers; }
        }
        
        /// <summary>
        /// Sanitizes the properties based on constraints.
        /// </summary>
        internal void Sanitize()
        {
            if (this.Address == null)
            {
                this.Address = string.Empty;
            }
        }
    }
}
