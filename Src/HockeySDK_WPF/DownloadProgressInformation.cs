using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.HockeyApp
{
    /// <summary>
    /// Download Progress Information class.
    /// </summary>
    public class DownloadProgressInformation
    {
        internal DownloadProgressInformation(int progressPercentage, long bytesReceived, long totalBytesToReceive)
        {
            this.ProgressPercentage = progressPercentage;
            this.BytesReceived = bytesReceived;
            this.TotalBytesToReceive = totalBytesToReceive;
        }

        /// <summary>
        /// Gets or sets progress percentage.
        /// </summary>
        public int ProgressPercentage{get;private set;}

        /// <summary>
        /// Gets or sets bytes received.
        /// </summary>
        public long BytesReceived{get;private set;}

        /// <summary>
        /// Gets or sets total bytes received.
        /// </summary>
        public long TotalBytesToReceive{get;private set;}
    }
}
