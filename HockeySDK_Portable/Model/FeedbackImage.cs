using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HockeyApp.Model
{
    public class FeedbackImage : IFeedbackImage
    {
        public FeedbackImage(string fileName, byte[] imageBytes, string contentType = "image/png")
        {
            this.FileName = fileName;
            this.ContentType = contentType;
            this.ImageBytes = imageBytes;
        }

        internal FeedbackImage(string fileName, byte[] imageBytes, string contentType, string remoteUrl)
        {
            this.FileName = fileName;
            this.ContentType = contentType;
            this.ImageBytes = imageBytes;
            this.RemoteURL = remoteUrl;
        }

        public string RemoteURL { get; internal set; }

        public string ContentType { get; internal set; }

        public string FileName { get; set; }
        
        public byte[] ImageBytes { get; set; }
    }
}
