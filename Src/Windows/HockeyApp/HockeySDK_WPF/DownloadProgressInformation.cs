using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HockeyApp
{
    public class DownloadProgressInformation
    {
        internal DownloadProgressInformation(int progressPercentage, long bytesReceived, long totalBytesToReceive)
        {
            this.ProgressPercentage = progressPercentage;
            this.BytesReceived = bytesReceived;
            this.TotalBytesToReceive = totalBytesToReceive;
        }
        public int ProgressPercentage{get;private set;}
        public long BytesReceived{get;private set;}
        public long TotalBytesToReceive{get;private set;}
    }
}
