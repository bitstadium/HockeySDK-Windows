// Based on https://github.com/kiwidev/WinRTExceptions/blob/master/Source/ExceptionHandlingSynchronizationContext.cs
// Copyright (c) 2012 Mark Young
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
// and associated documentation files (the "Software"), to deal in the Software without restriction, 
// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial
// portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
// LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE 
// OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Threading;
using System.Windows.Controls;

namespace Microsoft.HockeyApp
{
    /// <summary>
    /// Wrapper around a standard synchronization context, that catches any unhandled exceptions.
    /// Acts as a facade passing calls to the original SynchronizationContext
    /// </summary>
    public class AsyncSynchronizationContext : SynchronizationContext
    {
        private static CrashHandler crashHandler;

        /// <summary>
        /// Registration method.  Call this from OnLaunched and OnActivated inside the App.xaml.cs
        /// </summary>
        /// <returns></returns>
        public static AsyncSynchronizationContext Register()
        {
            var syncContext = Current;
            if (syncContext == null)
                throw new InvalidOperationException("Ensure a synchronization context exists before calling this method.");

            var customSynchronizationContext = syncContext as AsyncSynchronizationContext;

            if (customSynchronizationContext == null)
            {
                customSynchronizationContext = new AsyncSynchronizationContext(syncContext);
                SetSynchronizationContext(customSynchronizationContext);
            }
            return customSynchronizationContext;
        }

        /// <summary>
        /// Links the synchronization context to the specified frame
        /// and ensures that it is still in use after each navigation event
        /// </summary>
        /// <param name="rootFrame">The root frame.</param>
        /// <param name="handler">The handler.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">rootFrame</exception>
        public static AsyncSynchronizationContext RegisterForFrame(Frame rootFrame, CrashHandler handler)
        {
            if (rootFrame == null)
                throw new ArgumentNullException("rootFrame");

            crashHandler = handler;

            var synchronizationContext = Register();

            rootFrame.Navigating += (sender, args) => EnsureContext(synchronizationContext);
            rootFrame.Loaded += (sender, args) => EnsureContext(synchronizationContext);

            return synchronizationContext;
        }

        private static void EnsureContext(SynchronizationContext context)
        {
            if (Current != context)
                SetSynchronizationContext(context);
        }


        private readonly SynchronizationContext _syncContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncSynchronizationContext"/> class.
        /// </summary>
        /// <param name="syncContext">The synchronize context.</param>
        public AsyncSynchronizationContext(SynchronizationContext syncContext)
        {
            _syncContext = syncContext;
        }

        /// <summary>
        /// When overridden in a derived class, creates a copy of the synchronization context.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Threading.SynchronizationContext" /> object.
        /// </returns>
        public override SynchronizationContext CreateCopy()
        {
            return new AsyncSynchronizationContext(_syncContext.CreateCopy());
        }

        /// <summary>
        /// When overridden in a derived class, responds to the notification that an operation has completed.
        /// </summary>
        public override void OperationCompleted()
        {
            _syncContext.OperationCompleted();
        }

        /// <summary>
        /// When overridden in a derived class, responds to the notification that an operation has started.
        /// </summary>
        public override void OperationStarted()
        {
            _syncContext.OperationStarted();
        }

        /// <summary>
        /// When overridden in a derived class, dispatches an asynchronous message to a synchronization context.
        /// </summary>
        /// <param name="d">The <see cref="T:System.Threading.SendOrPostCallback" /> delegate to call.</param>
        /// <param name="state">The object passed to the delegate.</param>
        public override void Post(SendOrPostCallback d, object state)
        {
            _syncContext.Post(WrapCallback(d), state);
        }

        /// <summary>
        /// When overridden in a derived class, dispatches a synchronous message to a synchronization context.
        /// </summary>
        /// <param name="d">The <see cref="T:System.Threading.SendOrPostCallback" /> delegate to call.</param>
        /// <param name="state">The object passed to the delegate.</param>
        public override void Send(SendOrPostCallback d, object state)
        {
            _syncContext.Send(d, state);
        }


        private SendOrPostCallback WrapCallback(SendOrPostCallback sendOrPostCallback)
        {
            return state =>
            {
                try
                {
                    sendOrPostCallback(state);
                }
                catch (Exception ex)
                {
                    if (crashHandler != null)
                    {
                        crashHandler.HandleException(ex);
                        if (HockeyClientWP8SLExtension.customUnobservedTaskExceptionAction != null)
                        {
                            HockeyClientWP8SLExtension.customUnobservedTaskExceptionAction(ex);
                        }
                    }
                }
            };
        }

    }
}

