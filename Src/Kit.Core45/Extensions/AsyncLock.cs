using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.HockeyApp.Extensions
{
    /// <summary>
    /// Helper class to implement async locking
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "ToDo in future after integration of HockeyApp / Application Insights SDK is done")]
    internal sealed class AsyncLock
    {
        private readonly SemaphoreSlim m_semaphore = new SemaphoreSlim(1, 1);
        private readonly Task<IDisposable> m_releaser;

        /// <summary>
        /// constructor for a lock
        /// </summary>
        public AsyncLock()
        {
            m_releaser = Task.FromResult((IDisposable)new Releaser(this));
        }

        /// <summary>
        /// Get the lock.
        /// Usage: using(var lck = await myAsyncLock.LockAsync()) { ... }
        /// </summary>
        /// <returns>awaitable task</returns>
        public Task<IDisposable> LockAsync()
        {
            var wait = m_semaphore.WaitAsync();
            return wait.IsCompleted ?
                        m_releaser :
                        wait.ContinueWith((_, state) => (IDisposable)state,
                            m_releaser.Result, CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        private sealed class Releaser : IDisposable
        {
            private readonly AsyncLock m_toRelease;
            internal Releaser(AsyncLock toRelease) { m_toRelease = toRelease; }
            public void Dispose() { m_toRelease.m_semaphore.Release(); }
        }
    }
}
