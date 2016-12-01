namespace Microsoft.HockeyApp.Services
{
    using System;
    using System.Windows.Forms;
    using System.Threading.Tasks;
    using System.Globalization;
    using System.IO;
    using System.Collections.Generic;
    using System.Diagnostics;

    using Channel;
    using Device;

    using DataContracts;

    using Extensibility.Implementation.Tracing;
    using Extensibility.Implementation;

    internal sealed class UnhandledExceptionTelemetryModule : IUnhandledExceptionTelemetryModule
    {
        private bool initialized;
        private static ushort? processorArchitecture;

        internal UnhandledExceptionTelemetryModule(bool keepRunningAfterException)
        {
            if (keepRunningAfterException)
            {
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            }
        }

        public void Initialize()
        {
            if (!initialized)
            {
                Application.ThreadException += (o, e) => TrackUnhandledException(e.Exception, "Application.ThreadException");
                AppDomain.CurrentDomain.UnhandledException += (o, e) => TrackUnhandledException(e.ExceptionObject as Exception, "CurrentDomain.UnhandledException");
                TaskScheduler.UnobservedTaskException += (o, e) => TrackUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");

                initialized = true;
            }
        }

        private void TrackUnhandledException(Exception e, string source)
        {
            if (e != null)
            {
                CoreEventSource.Log.LogVerbose("UnhandledExceptionTelemetryModule." + source + " started successfully");
                try
                {
                    var crashTelemetry = CreateCrashTelemetry(e, ExceptionHandledAt.Unhandled);
                    var client = ((HockeyClient)(HockeyClient.Current));
                    client.Track(crashTelemetry);
                    client.Flush();
                }
                catch (Exception ex)
                {
                    CoreEventSource.Log.LogError("An exeption occured in UnhandledExceptionTelemetryModule." + source + " " + ex);
                }
            }
        }

        public ITelemetry CreateCrashTelemetry(Exception exception, ExceptionHandledAt handledAt)
        {
            CrashTelemetry result = new CrashTelemetry();
            result.HandledAt = handledAt;
            result.Headers.Id = Guid.NewGuid().ToString("D");
            result.Headers.CrashThreadId = Environment.CurrentManagedThreadId;
            result.Headers.ExceptionType = exception.GetType().FullName;
            result.Headers.ExceptionReason = exception.Message;

            var description = string.Empty;
            if (HockeyClient.Current.AsInternal().DescriptionLoader != null)
            {
                try
                {
                    result.Attachments.Description = HockeyClient.Current.AsInternal().DescriptionLoader(exception);
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

            // TODO - based on the implementation of StackFrameExtensions.cs in this project this code section will never execute

            // stackTrace.GetFrames may return null (happened on Outlook Groups application). 
            // HasNativeImage() method invoke on first frame is required to understand whether an application is compiled in native tool chain
            // and we can extract the frame addresses or not.
            if (frames != null && frames.Length > 0 && frames[0].HasNativeImage())
            {
                foreach (StackFrame frame in stackTrace.GetFrames())
                {
                    CrashTelemetryThreadFrame crashFrame = new CrashTelemetryThreadFrame
                    {
                        Address = string.Format(CultureInfo.InvariantCulture, "0x{0:x16}", frame.GetNativeIP().ToInt64()),
                        Symbol = string.Format(CultureInfo.InvariantCulture, "   at {0}.{1}  (0x{2:x8}, 0x{3:x})", frame.GetMethod().DeclaringType.FullName, frame.GetMethod().Name, frame.GetMethod().MetadataToken, frame.GetILOffset())
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
                        CpuType = processorArchitecture ?? (long)(processorArchitecture = DeviceService.GetProcessorArchitecture())
                    };

                    result.Binaries.Add(crashBinary);
                    seenBinaries.Add(nativeImageBase);
                }
            }

            result.StackTrace = GetStrackTrace(exception);
            return result;
        }

        private static string GetStrackTrace(Exception e)
        {
            CultureInfo originalUICulture = CultureInfo.CurrentUICulture;
            try
            {
                // we need to switch to invariant culture, because stack trace localized and we cannot parse it efficiently on the server side.
                // see https://support.hockeyapp.net/discussions/problems/58504-non-english-stack-trace-not-displayed
                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
                return e.StackTrace;
            }
            finally
            {
                CultureInfo.DefaultThreadCurrentUICulture = originalUICulture;
            }
        }
    }
}
