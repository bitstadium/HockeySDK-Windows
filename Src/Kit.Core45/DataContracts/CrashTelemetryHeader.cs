namespace Microsoft.HockeyApp.DataContracts
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Microsoft.HockeyApp.Channel;
    using Microsoft.HockeyApp.Extensibility.Implementation;
    using Microsoft.HockeyApp.Extensibility.Implementation.External;

    /// <summary>
    /// The type used to specify header properties of a crash.
    /// </summary>
    internal sealed class CrashTelemetryHeaders
    {
        internal readonly CrashDataHeaders Data;

        /// <summary>
        /// Initializes a new instance of the <see cref="CrashTelemetryHeaders" /> class.
        /// </summary>
        internal CrashTelemetryHeaders()
            : this(new CrashDataHeaders())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CrashTelemetryHeaders" /> class.
        /// </summary>
        /// <param name="headers">The headers.</param>
        internal CrashTelemetryHeaders(CrashDataHeaders headers)
        {
            this.Data = headers ?? new CrashDataHeaders();
        }

        /// <summary>
        /// Gets or sets the identifier for this crash.
        /// </summary>
        public string Id
        {
            get { return this.Data.id; }
            set { this.Data.id = value; }
        }

        /// <summary>
        /// Gets or sets the process name for this crash.
        /// </summary>
        public string Process
        {
            get { return this.Data.process; }
            set { this.Data.process = value; }
        }

        /// <summary>
        /// Gets or sets the process identifier.
        /// </summary>
        public ushort ProcessId
        {
            get { return (ushort)this.Data.processId; }
            set { this.Data.processId = value; }
        }

        /// <summary>
        /// Gets or sets the parent process name.
        /// </summary>
        public string ParentProcess
        {
            get { return this.Data.parentProcess; }
            set { this.Data.parentProcess = value; }
        }

        /// <summary>
        /// Gets or sets the parent process identifier.
        /// </summary>
        public ushort ParentProcessId
        {
            get { return (ushort)this.Data.parentProcessId; }
            set { this.Data.parentProcessId = value; }
        }

        /// <summary>
        /// Gets or sets the crash thread identifier.
        /// </summary>>
        public int CrashThreadId
        {
            get { return (ushort)this.Data.crashThread; }
            set { this.Data.crashThread = value; }
        }

        /// <summary>
        /// Gets or sets the application path.
        /// </summary>
        public string ApplicationPath
        {
            get { return this.Data.applicationPath; }
            set { this.Data.applicationPath = value; }
        }

        /// <summary>
        /// Gets or sets the application identifier.
        /// </summary>
        public string ApplicationId
        {
            get { return this.Data.applicationIdentifier; }
            set { this.Data.applicationIdentifier = value; }
        }

        /// <summary>
        /// Gets or sets the type of the exception.
        /// </summary>
        public string ExceptionType
        {
            get { return this.Data.exceptionType; }
            set { this.Data.exceptionType = value; }
        }

        /// <summary>
        /// Gets or sets the exception code.
        /// </summary>
        public string ExceptionCode
        {
            get { return this.Data.exceptionCode; }
            set { this.Data.exceptionCode = value; }
        }

        /// <summary>
        /// Gets or sets the exception address.
        /// </summary>
        public string ExceptionAddress
        {
            get { return this.Data.exceptionAddress; }
            set { this.Data.exceptionAddress = value; }
        }

        /// <summary>
        /// Gets or sets the exception reason.
        /// </summary>
        public string ExceptionReason
        {
            get { return this.Data.exceptionReason; }
            set { this.Data.exceptionReason = value; }
        }

        /// <summary>
        /// Sanitizes the properties based on constraints.
        /// </summary>
        internal void Sanitize()
        {
            if (string.IsNullOrEmpty(this.Id) == true)
            {
                this.Id = Guid.NewGuid().ToString("D");
            }
        }
    }
}
