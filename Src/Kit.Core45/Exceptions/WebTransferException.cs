using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.HockeyApp.Exceptions
{

    /// <summary>
    /// Namespace for custom exceptions
    /// </summary>
    internal class NamespaceDoc { }

    /// <summary>
    /// Exception used for indication an exception during datatransfer to the server like a connection timeout. ( => Try again later )
    /// </summary>
    internal class WebTransferException : Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public WebTransferException() : base() { }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Exception message</param>
        public WebTransferException(string message) : base(message) { }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="inner">Exception causing the exception</param>
        public WebTransferException(string message, System.Exception inner) : base(message, inner) { }
        
    }
}
