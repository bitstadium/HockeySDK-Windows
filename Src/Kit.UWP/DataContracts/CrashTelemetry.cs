namespace Microsoft.HockeyApp.DataContracts
{
    using Microsoft.HockeyApp.Extensibility.Implementation;
    using Extensibility.Implementation.Tracing;
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

            // ToDo: Clarify what does ApplicationPath and Process needs to contain.
            this.Headers.ApplicationPath = "N/A";
            this.Headers.Process = "N/A";
            this.headers.ApplicationId = "N/A";
            this.headers.ParentProcess = "N/A";
            this.headers.ExceptionCode = "N/A";
            this.headers.ExceptionAddress = "N/A";

            var description = string.Empty;
            if (TelemetryConfiguration.Active.DescriptionLoader != null)
            {
                try
                {
                    this.Attachments.Description = TelemetryConfiguration.Active.DescriptionLoader(exception);
                }
                catch (Exception)
                {
                    CoreEventSource.Log.LogVerbose("DescriptionLoader callback fired an exception: " + exception);
                }
            }

            CrashTelemetryThread thread = new CrashTelemetryThread
                                                {
                                                    Id = Environment.CurrentManagedThreadId
                                                };
            this.Threads.Add(thread);

            // we can extract stack frames only if application is compiled with native tool chain.
            if (Extensibility.DeviceContextReader.IsNativeEnvironment(exception))
            {
                HashSet<long> seenBinaries = new HashSet<long>();

                StackTrace stackTrace = new StackTrace(exception, true);
                var frames = stackTrace.GetFrames();

                // stackTrace.GetFrames may return null (happened on Outlook Groups application). 
                if (frames != null)
                {
                    foreach (StackFrame frame in stackTrace.GetFrames())
                    {
                        CrashTelemetryThreadFrame crashFrame = new CrashTelemetryThreadFrame
                        {
                            Address = string.Format(CultureInfo.InvariantCulture, "0x{0:x16}", frame.GetNativeIP().ToInt64())
                        };
                        thread.Frames.Add(crashFrame);

                        long nativeImageBase = frame.GetNativeImageBase().ToInt64();
                        if (seenBinaries.Contains(nativeImageBase) == true)
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
                            StartAddress = string.Format(CultureInfo.InvariantCulture, "0x{0:x16}", nativeImageBase),
                            EndAddress = string.Format(CultureInfo.InvariantCulture, "0x{0:x16}", nativeImageBase),
                            Uuid = string.Format(CultureInfo.InvariantCulture, "{0:N}-{1}", codeView.Signature, codeView.Age),
                            Path = codeView.PdbPath,
                            Name = string.IsNullOrEmpty(codeView.PdbPath) == false ? Path.GetFileNameWithoutExtension(codeView.PdbPath) : null,
                            CpuType = Extensibility.DeviceContextReader.GetProcessorArchitecture()
                        };

                        this.Binaries.Add(crashBinary);
                        seenBinaries.Add(nativeImageBase);
                    }
                }
            }

            this.StackTrace = exception.StackTrace;
        }
    }
}
