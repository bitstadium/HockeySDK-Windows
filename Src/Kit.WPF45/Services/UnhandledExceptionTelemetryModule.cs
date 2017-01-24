using System.Threading;

namespace Microsoft.HockeyApp.Extensibility.Windows
{
    using System;
    using Channel;
    using DataContracts;
    using Implementation.Tracing;

    using Microsoft.HockeyApp.Extensions;

    using System.Collections.Generic;
    using Implementation;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using Services;
    using System.Threading.Tasks;
    using System.Windows.Threading;
    using System.Windows;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

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
        /// Subscribes to unhandled event notifications.
        /// </summary>
        public void Initialize()
        {
            AppDomain.CurrentDomain.UnhandledException += AppDomain_UnhandledException;

            // This is handled by the AppDomain_UnhandledException handler as well
            // Application.Current.DispatcherUnhandledException += Application_DispatcherUnhandledException;

            // This exception won't cause an application crash
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        /// <summary>
        /// Handles the DispatcherUnhandledException event of the Application's dispatcher.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DispatcherUnhandledExceptionEventArgs"/> instance containing the event data.</param>
        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            TrackException(e.Exception, ExceptionHandledAt.Unhandled);

            if (HockeyClientWPFExtensions.customDispatcherUnhandledExceptionAction != null)
            {
                HockeyClientWPFExtensions.customDispatcherUnhandledExceptionAction(e);
            }
        }

        /// <summary>
        /// Handles the UnhandledException event of the current app domain.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="UnhandledExceptionEventArgs"/> instance containing the event data.</param>
        private void AppDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            TrackException(e.ExceptionObject as Exception, ExceptionHandledAt.Unhandled);

            if (HockeyClientWPFExtensions.customUnhandledExceptionAction  != null)
            {
                HockeyClientWPFExtensions.customUnhandledExceptionAction(e);
            }
        }

        /// <summary>
        /// Handles the UnobservedTaskException event of the default task scheduler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="UnobservedTaskExceptionEventArgs"/> instance containing the event data.</param>
        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            TrackException(e.Exception, ExceptionHandledAt.Unhandled);

            if (HockeyClientWPFExtensions.customUnobservedTaskExceptionAction != null)
            {
                HockeyClientWPFExtensions.customUnobservedTaskExceptionAction(e);
            }
        }

        /// <summary>
        /// Tracks the exception.
        /// </summary>
        /// <param name="exception">The exception to initialize the class with.</param>
        /// <param name="handledAt">Determines whether exception is handled or unhandled.</param>
        private void TrackException(Exception exception, ExceptionHandledAt handledAt)
        {
            try
            {
                if (exception != null)
                {
                    ITelemetry crashTelemetry = CreateCrashTelemetry(exception, handledAt);
                    var client = ((HockeyClient)(HockeyClient.Current));
                    client.Track(crashTelemetry);
                    client.Flush();
                }
            }
            catch (Exception ex)
            {
                CoreEventSource.Log.LogError("An exeption occured in UnhandledExceptionTelemetryModule.TrackException: " + ex);
            }
        }

        /// <summary>
        /// Creates <see cref="CrashTelemetry"/> instance.
        /// </summary>
        /// <param name="exception">The exception to initialize the class with.</param>
        /// <param name="handledAt">Determines whether exception is handled or unhandled.</param>
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
                        CpuType = GetProcessorArchitecture()
                    };

                    result.Binaries.Add(crashBinary);
                    seenBinaries.Add(nativeImageBase);
                }
            }

            result.StackTrace = GetStackTrace(exception);

            return result;
        }

        /// <summary>
        /// Gets the stack trace of the exception in the invariant culture.
        /// </summary>
        /// <param name="e">The exception.</param>
        /// <returns>The culture independent stack trace.</returns>
        private static string GetStackTrace(Exception e)
        {
            var originalUICulture = CultureInfo.CurrentUICulture;
            try
            {
                // we need to switch to invariant culture, because stack trace localized and we cannot parse it efficiently on the server side.
                // see https://support.hockeyapp.net/discussions/problems/58504-non-english-stack-trace-not-displayed
                Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
                return e.StackTraceToString();
            }
            finally
            {
                Thread.CurrentThread.CurrentUICulture = originalUICulture;
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
                    processorArchitecture = (ushort)sysInfo.wProcessorArchitecture;
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