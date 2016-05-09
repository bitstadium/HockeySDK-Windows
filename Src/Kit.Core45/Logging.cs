using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.HockeyApp
{
    /// <summary>
    /// Universal log interface
    /// </summary>
    public interface ILog
    {
        /// <summary>
        /// Logs an info message
        /// </summary>
        /// <param name="format">message to log</param>
        /// <param name="args">Parameters, which can be injected to the message</param>
        void Info(string format, params object[] args);

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="format">message to log.</param>
        /// <param name="args">Parameters, which can be injected to the message</param>
        void Warn(string format, params object[] args);

        /// <summary>
        /// Logs an error message with an exception.
        /// </summary>
        /// <param name="exception">exception with has to be logged.</param>
        void Error(Exception exception);
    }

    /// <summary>
    /// Class for registering logging
    /// </summary>
    public static class HockeyLogManager
    {
        static readonly ILog NullLoggerInstance = new NullLogger();

        /// <summary>
        /// Creates an <see cref="ILog"/> for the provided type.
        /// </summary>
        public static Func<Type, ILog> GetLog = type => NullLoggerInstance;

        //Just a Logger, which is doing nothing
        class NullLogger : ILog
        {
            public void Info(string format, params object[] args) { }
            public void Warn(string format, params object[] args) { }
            public void Error(Exception exception) { }
        }
    }
}
