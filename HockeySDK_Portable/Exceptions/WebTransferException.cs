using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HockeyApp.Exceptions
{
    /// <summary>
    /// Exception used for indication an exception during datatransfer to the server like a connection timeout. ( => Try again later )
    /// </summary>
    public class WebTransferException : Exception
    {
        public WebTransferException() : base() { }
        public WebTransferException(string message) : base(message) { }
        public WebTransferException(string message, System.Exception inner) : base(message, inner) { }
        
    }
}
