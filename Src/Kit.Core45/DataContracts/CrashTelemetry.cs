namespace Microsoft.HockeyApp.DataContracts
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Microsoft.HockeyApp.Channel;
    using Microsoft.HockeyApp.Extensibility.Implementation;
    using Microsoft.HockeyApp.Extensibility.Implementation.External;

    /// <summary>
    /// Telemetry type used to track crashes.
    /// </summary>
    internal sealed partial class CrashTelemetry : ITelemetry
    {
        internal const string TelemetryName = "Crash";

        internal readonly string BaseType = typeof(CrashData).Name;
        internal readonly CrashData Data;
        private readonly CrashTelemetryHeaders headers;
        private readonly AdapterList<CrashTelemetryThread, CrashDataThread> adapterThreads;
        private readonly AdapterList<CrashTelemetryBinary, CrashDataBinary> adapterBinaries;
        private readonly TelemetryContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="CrashTelemetry"/> class.
        /// </summary>
        public CrashTelemetry()
            : this(new CrashData())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CrashTelemetry" /> class.
        /// </summary>
        /// <param name="crash">The crash.</param>
        internal CrashTelemetry(CrashData crash)
        {
            this.Data = crash ?? new CrashData();
            if (this.Data.headers == null)
            {
                this.Data.headers = new CrashDataHeaders();
            }

            this.headers = new CrashTelemetryHeaders(this.Data.headers);
            this.adapterThreads = new AdapterList<CrashTelemetryThread, CrashDataThread>(
                                                                new List<CrashTelemetryThread>(),
                                                                this.Data.threads,
                                                                threadPublic => threadPublic == null ? null : threadPublic.Data,
                                                                threadPrivate => threadPrivate == null ? null : new CrashTelemetryThread(threadPrivate));
            this.adapterThreads.SyncPrivateToPublic();

            this.adapterBinaries = new AdapterList<CrashTelemetryBinary, CrashDataBinary>(
                                                                new List<CrashTelemetryBinary>(),
                                                                this.Data.binaries,
                                                                binaryPublic => binaryPublic == null ? null : binaryPublic.Data,
                                                                binaryPrivate => binaryPrivate == null ? null : new CrashTelemetryBinary(binaryPrivate));
            this.adapterBinaries.SyncPrivateToPublic();

            this.context = new TelemetryContext(new Dictionary<string, string>(), new Dictionary<string, string>());
            this.Attachments = new Attachments();
        }

        /// <summary>
        /// Gets or sets date and time when event was recorded.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the value that defines absolute order of the telemetry item.
        /// </summary>
        public string Sequence { get; set; }

        /// <summary>
        /// Gets the context associated with the current telemetry item.
        /// </summary>
        public TelemetryContext Context
        {
            get { return this.context; }
        }

        /// <summary>
        /// Gets the headers.
        /// </summary>
        public CrashTelemetryHeaders Headers
        {
            get { return this.headers; }
        }

        /// <summary>
        /// Gets the threads.
        /// </summary>
        public IList<CrashTelemetryThread> Threads
        {
            get { return this.adapterThreads; }
        }

        /// <summary>
        /// Gets the binaries.
        /// </summary>
        public IList<CrashTelemetryBinary> Binaries
        {
            get { return this.adapterBinaries; }
        }

        public Attachments Attachments
        {
            get;
        }

        /// <summary>
        /// Gets or sets the value indicated where the exception was handled.
        /// </summary>
        public ExceptionHandledAt HandledAt
        {
            get
            {
                ExceptionHandledAt result;
                return Enum.TryParse<ExceptionHandledAt>(this.Data.handledAt, out result) ? result : ExceptionHandledAt.Unhandled;
            }

            set
            {
                this.Data.handledAt = value.ToString();
            }
        }

        public string StackTrace
        {
            get; set;
        }

        /// <summary>
        /// Sanitizes the properties based on constraints.
        /// </summary>
        void ITelemetry.Sanitize()
        {
            this.headers.Sanitize();

            this.adapterThreads.PublicCollection.SanitizeCollection(item => item.Sanitize());
            this.adapterThreads.SyncPublicToPrivate();

            this.adapterBinaries.PublicCollection.SanitizeCollection(item => item.Sanitize());
            this.adapterBinaries.SyncPublicToPrivate();
        }
    }
}
