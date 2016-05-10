namespace Microsoft.HockeyApp.Extensibility.Windows
{
    using System;
    using System.Threading;

    using Channel;
    using DataContracts;
    using Extensibility;
    using Extensibility.Implementation.Tracing;

    using global::Windows.ApplicationModel.Core;
    using global::Windows.UI.Xaml;
    using System.Collections.Generic;
    using Microsoft.HockeyApp.Extensibility.Implementation;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using Services;
    
    /// <summary>
    /// A module that deals in Exception events and will create ExceptionTelemetry objects when triggered.
    /// </summary>
    internal sealed class UnhandledExceptionTelemetryModule : IUnhandledExceptionTelemetryModule
    {
        private TelemetryClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnhandledExceptionTelemetryModule"/> class.
        /// </summary>
        internal UnhandledExceptionTelemetryModule()
        {
        }
        
        internal bool AlwaysHandleExceptions { get; set; }
        
        /// <summary>
        /// Unsubscribe from the <see cref="Application.UnhandledException"/> event.
        /// </summary>
        public void Dispose()
        {
            CoreApplication.UnhandledErrorDetected -= CoreApplication_UnhandledErrorDetected;
        }

        /// <summary>
        /// Subscribes to unhandled event notifications.
        /// We are using <see cref="CoreApplication.UnhandledErrorDetected"/> instead of 
        /// <see cref="Application.UnhandledException"/> because <see cref="Application.UnhandledException"/> is not idempotent and 
        /// the exception object may be read only once. The second time it is read, it will return empty <see cref="System.Exception"/> without call stack.
        /// It is OS Bug 560663, 7133918 that must be fixed in Windows Redstone 2 (~2017).
        /// </summary>
        public void Initialize(TelemetryConfiguration configuration)
        {
            CoreApplication.UnhandledErrorDetected += CoreApplication_UnhandledErrorDetected;
        }

        private void CoreApplication_UnhandledErrorDetected(object sender, UnhandledErrorDetectedEventArgs e)
        {
            global::System.Diagnostics.Debug.WriteLine("UnhandledExceptionTelemetryModule.CoreApplication_UnhandledErrorDetected started successfully");
            try
            {
                // intentionally propagating exception to get the exception object that crashed the app.
                e.UnhandledError.Propagate();
            }
            catch (Exception eventException)
            {
                try
                {
                    LazyInitializer.EnsureInitialized(ref this.client, () => { return new TelemetryClient(); });
                    ITelemetry crashTelemetry = CreateCrashTelemetry(eventException);
                    this.client.Track(crashTelemetry);
                    this.client.Flush();
                }
                catch (Exception ex)
                {
                    CoreEventSource.Log.LogError("An exeption occured in UnhandledExceptionTelemetryModule.CoreApplication_UnhandledErrorDetected: " + ex);
                }

                // if we don't throw exception - app will not be crashed. We need to throw to not change the app behavior.
                // known issue: stack trace will contain SDK methods from now on.
                throw;
            }
        }

        /// <summary>
        /// Creates <see cref="CrashTelemetry"/> instance.
        /// </summary>
        /// <param name="exception">The exception to initialize the class with.</param>
        private CrashTelemetry CreateCrashTelemetry(Exception exception)
        {
            CrashTelemetry result = new CrashTelemetry();
            result.HandledAt = ExceptionHandledAt.Unhandled;
            result.Headers.Id = Guid.NewGuid().ToString("D");
            result.Headers.CrashThreadId = Environment.CurrentManagedThreadId;
            result.Headers.ExceptionType = exception.GetType().FullName;
            result.Headers.ExceptionReason = exception.Message;

            var description = string.Empty;
            if (TelemetryConfiguration.Active.DescriptionLoader != null)
            {
                try
                {
                    result.Attachments.Description = TelemetryConfiguration.Active.DescriptionLoader(exception);
                }
                catch (Exception ex)
                {
                    CoreEventSource.Log.LogError("An exception occured in TelemetryConfiguration.Active.DescriptionLoader callback : " + ex);
                }
            }

            CrashTelemetryThread thread = new CrashTelemetryThread { Id = Environment.CurrentManagedThreadId };
            result.Threads.Add(thread);
            HashSet<long> seenBinaries = new HashSet<long>();

            StackTrace stackTrace = new StackTrace(exception, true);
            var frames = stackTrace.GetFrames();

            // stackTrace.GetFrames may return null (happened on Outlook Groups application). 
            // HasNativeImage() method invoke on first frame is required to understand whether an application is compiled in native tool chain
            // and we can extract the frame addresses or not.
            if (frames != null && frames.Length > 0 && frames[0].HasNativeImage())
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
                        EndAddress = string.Format(CultureInfo.InvariantCulture, "0x{0:x16}", codeView.EndAddress.ToInt64()),
                        Uuid = string.Format(CultureInfo.InvariantCulture, "{0:N}-{1}", codeView.Signature, codeView.Age),
                        Path = codeView.PdbPath,
                        Name = string.IsNullOrEmpty(codeView.PdbPath) == false ? Path.GetFileNameWithoutExtension(codeView.PdbPath) : null,
                        CpuType = Extensibility.DeviceContextReader.GetProcessorArchitecture()
                    };

                    result.Binaries.Add(crashBinary);
                    seenBinaries.Add(nativeImageBase);
                }
            }

            result.StackTrace = exception.StackTrace;
            return result;
        }
    }
}
