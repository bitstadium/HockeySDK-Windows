namespace Microsoft.HockeyApp.DataContracts
{
    using Microsoft.HockeyApp.Extensibility.Implementation;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;


    /// <summary>
    /// Telemetry type used to track crashes.
    /// </summary>
    internal sealed partial class CrashTelemetry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CrashTelemetry"/> class.
        /// </summary>
        /// <param name="exception">The exception to initialize the class with.</param>
        public CrashTelemetry(Exception exception)
            : this()
        {
            if (exception == null)
            {
                exception = new Exception(Utils.PopulateRequiredStringValue(null, "message", typeof(ExceptionTelemetry).FullName));
            }

            this.InitializeFromException(exception);
        }

        /// <summary>
        /// Initializes the current instance with respect to passed in exception.
        /// </summary>
        /// <param name="exception">The exception to initialize the current instance with.</param>
        private void InitializeFromException(Exception exception)
        {
            this.Headers.Id = Guid.NewGuid().ToString("D");
            this.Headers.CrashThreadId = Environment.CurrentManagedThreadId;
            this.Headers.ExceptionType = exception.GetType().FullName;
            this.Headers.ExceptionReason = exception.Message;

            CrashTelemetryThread thread = new CrashTelemetryThread
                                                {
                                                    Id = Environment.CurrentManagedThreadId
                                                };
            this.Threads.Add(thread);

            HashSet<long> seenBinaries = new HashSet<long>();

            StackTrace stackTrace = new StackTrace(exception, true);
            foreach (StackFrame frame in stackTrace.GetFrames())
            {
                CrashTelemetryThreadFrame crashFrame = new CrashTelemetryThreadFrame
                                                            {
                                                                Address = string.Format(CultureInfo.InvariantCulture, "0x{0:x16}", frame.GetNativeIP().ToInt64())
                                                            };
                thread.Frames.Add(crashFrame);

                long nativeIamgeBase = frame.GetNativeImageBase().ToInt64();
                if (seenBinaries.Contains(nativeIamgeBase) == true)
                {
                    continue;
                }

                PEImageReader reader = new PEImageReader(frame.GetNativeImageBase());
                PEImageReader.CodeViewDebugData codeView = reader.Parse();
                if (codeView == null)
                {
                    continue;
                }

                CrashTelemetryBinary crashBinary = new CrashTelemetryBinary
                                                        {
                                                            StartAddress = string.Format(CultureInfo.InvariantCulture, "0x{0:x16}", nativeIamgeBase),
                                                            Uuid = string.Format(CultureInfo.InvariantCulture, "{0:N}-{1}", codeView.Signature, codeView.Age),
                                                            Path = codeView.PdbPath,
                                                            Name = string.IsNullOrEmpty(codeView.PdbPath) == false ? Path.GetFileNameWithoutExtension(codeView.PdbPath) : null
                                                        };

                this.Binaries.Add(crashBinary);
                seenBinaries.Add(nativeIamgeBase);
            }

            this.StackTrace = exception.StackTrace;
        }
    }
}
