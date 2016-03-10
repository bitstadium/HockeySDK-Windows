namespace Microsoft.HockeyApp.DataContracts
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Microsoft.HockeyApp.Channel;
    using Microsoft.HockeyApp.Extensibility.Implementation;
    using Microsoft.HockeyApp.Extensibility.Implementation.External;

    /// <summary>
    /// The type used to specify thread properties of a crash.
    /// </summary>
    internal sealed class CrashTelemetryThread
    {
        internal readonly CrashDataThread Data;
        private readonly AdapterList<CrashTelemetryThreadFrame, CrashDataThreadFrame> adapterFrames;
 
        /// <summary>
        /// Initializes a new instance of the <see cref="CrashTelemetryThread"/> class.
        /// </summary>
        public CrashTelemetryThread()
            : this(new CrashDataThread())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CrashTelemetryThread" /> class.
        /// </summary>
        /// <param name="thread">The thread.</param>
        internal CrashTelemetryThread(CrashDataThread thread)
        {
            this.Data = thread ?? new CrashDataThread();
            this.adapterFrames = new AdapterList<CrashTelemetryThreadFrame, CrashDataThreadFrame>(
                                                                new List<CrashTelemetryThreadFrame>(),
                                                                this.Data.frames,
                                                                framePublic => framePublic == null ? null : framePublic.Data,
                                                                framePrivate => framePrivate == null ? null : new CrashTelemetryThreadFrame(framePrivate));

            this.adapterFrames.SyncPrivateToPublic();
        }

        /// <summary>
        /// Gets or sets the identifier for this thread.
        /// </summary>
        public int Id
        {
            get { return this.Data.id; }
            set { this.Data.id = value; }
        }

        /// <summary>
        /// Gets the set of frames that have been captured for this thread.
        /// </summary>
        public IList<CrashTelemetryThreadFrame> Frames
        {
            get { return this.adapterFrames; }
        }
        
        /// <summary>
        /// Sanitizes the properties based on constraints.
        /// </summary>
        internal void Sanitize()
        {
            this.adapterFrames.PublicCollection.SanitizeCollection(item => item.Sanitize());
            this.adapterFrames.SyncPublicToPrivate();
        }
    }
}
