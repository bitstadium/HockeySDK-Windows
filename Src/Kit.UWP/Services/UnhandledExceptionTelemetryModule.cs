namespace Microsoft.HockeyApp.Extensibility.Windows
{
    using System;
    using Channel;
    using DataContracts;
    using Implementation.Tracing;

    using global::Windows.ApplicationModel.Core;
    using global::Windows.UI.Xaml;
    using System.Collections.Generic;
    using Implementation;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using Services;
    using Services.Device;
    using System.Threading.Tasks;

    /// <summary>
    /// A module that deals in Exception events and will create ExceptionTelemetry objects when triggered.
    /// </summary>
    internal sealed class UnhandledExceptionTelemetryModule : IUnhandledExceptionTelemetryModule
    {
        private static ushort? processorArchitecture;

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
            TaskScheduler.UnobservedTaskException -= TaskScheduler_UnobservedTaskException;
        }

        /// <summary>
        /// Subscribes to unhandled event notifications.
        /// We are using <see cref="CoreApplication.UnhandledErrorDetected"/> instead of 
        /// <see cref="Application.UnhandledException"/> because <see cref="Application.UnhandledException"/> is not idempotent and 
        /// the exception object may be read only once. The second time it is read, it will return empty <see cref="System.Exception"/> without call stack.
        /// It is OS Bug 560663, 7133918 that must be fixed in Windows Redstone 2 (~2017).
        /// </summary>
        public void Initialize()
        {
            CoreApplication.UnhandledErrorDetected += CoreApplication_UnhandledErrorDetected;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException; 
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            // Imlementing this feature based on this request 
            // https://support.hockeyapp.net/discussions/problems/58502-uwp-how-to-call-trackexception?mail_type=queue

            try
            {
                if (e.Exception != null)
                {
                    ITelemetry crashTelemetry = CreateCrashTelemetry(e.Exception, ExceptionHandledAt.Unhandled);
                    var client = ((HockeyClient)(HockeyClient.Current));
                    client.Track(crashTelemetry);
                    client.Flush();
                }
            }
            catch(Exception ex)
            {
                CoreEventSource.Log.LogError("An exeption occured in UnhandledExceptionTelemetryModule.TaskScheduler_UnobservedTaskException: " + ex);
            }
        }

        /// <summary>
        /// Creates <see cref="CrashTelemetry"/> instance.
        /// </summary>
        /// <param name="exception">The exception to initialize the class with.</param>
        /// <param name="handledAt">Determines whether exception is handled or unhandled.</param>
        public ITelemetry CreateCrashTelemetry(Exception exception, ExceptionHandledAt handledAt)
        {
            exception = FlattenAggregateException(exception);

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
                        CpuType = GetProcessorArchitecture()
                    };

                    result.Binaries.Add(crashBinary);
                    seenBinaries.Add(nativeImageBase);
                }
            }

            result.StackTrace = GetStrackTrace(exception);
            return result;
        }

        private Exception FlattenAggregateException(Exception e)
        {
            // Flatten the AggregateException and unwrap it if we only have a single inner exception
            var aggregateException = e as AggregateException;
            if (aggregateException != null)
            {
                aggregateException = aggregateException.Flatten();
                if (aggregateException.InnerException != null)
                {
                    e = aggregateException.InnerException;
                }
                else if (aggregateException.InnerExceptions.Count == 1)
                {
                    e = aggregateException.InnerExceptions[0];
                }
            }
            return e;
        }

        private string GetStrackTrace(Exception e)
        {
            CultureInfo originalUICulture = CultureInfo.CurrentUICulture;
            try
            {
                // we need to switch to invariant culture, because stack trace localized and we cannot parse it efficiently on the server side.
                // see https://support.hockeyapp.net/discussions/problems/58504-non-english-stack-trace-not-displayed
                CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
                return e.StackTrace;
            }
            finally
            {
                CultureInfo.CurrentUICulture = originalUICulture;
            }
        }

        private void CoreApplication_UnhandledErrorDetected(object sender, UnhandledErrorDetectedEventArgs e)
        {
            CoreEventSource.Log.LogVerbose("UnhandledExceptionTelemetryModule.CoreApplication_UnhandledErrorDetected started successfully");

            try
            {
                // intentionally propagating exception to get the exception object that crashed the app.
                e.UnhandledError.Propagate();
            }
            catch (Exception eventException)
            {
                try
                {

                    ITelemetry crashTelemetry = CreateCrashTelemetry(eventException, ExceptionHandledAt.Unhandled);
                    var client = ((HockeyClient)(HockeyClient.Current));
                    client.Track(crashTelemetry);
                    client.Flush();
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
        /// Get the processor architecture of this computer.
        /// </summary>
        /// <remarks>
        /// This method cannot be used in SDK other than UWP, because it is using <see cref="NativeMethods.GetNativeSystemInfo(ref NativeMethods._SYSTEM_INFO)"/> 
        /// API, which violates Windows Phone certification requirements for WinRT platform, see https://www.yammer.com/microsoft.com/#/uploaded_files/59829318?threadId=718448267
        /// </remarks>
        /// <returns>The processor architecture of this computer. </returns>
        private static ushort GetProcessorArchitecture()
        {
            if (!processorArchitecture.HasValue)
            {
                try
                {
                    var sysInfo = new NativeMethods._SYSTEM_INFO();
                    NativeMethods.GetNativeSystemInfo(ref sysInfo);
                    processorArchitecture = sysInfo.wProcessorArchitecture;
                }
                catch
                {
                    // unknown architecture.
                    processorArchitecture = 0xffff;
                }
            }

            // https://msdn.microsoft.com/en-us/library/windows/desktop/ms724958(v=vs.85).aspx
            return processorArchitecture.Value;
        }
    }
}
