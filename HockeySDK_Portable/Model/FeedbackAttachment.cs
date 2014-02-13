using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace HockeyApp.Model
{
    [DataContract]
    public class FeedbackAttachment : IFeedbackAttachment
    {
        public FeedbackAttachment(string fileName, byte[] imageBytes, string contentType)
        {
            this.FileName = fileName;
            this.ContentType = contentType;
            this.DataBytes = imageBytes;
        }

        [DataMember(Name = "url")]
        public string RemoteURL { get; internal set; }

        [DataMember(Name = "created_at")]
        public string CreatedAt { get; internal set; }

        [DataMember(Name = "id")]
        public string Id { get; internal set; }
        
        [DataMember(Name = "file_name")]
        public string FileName { get; set; }

        public byte[] DataBytes { get; set; }
        public string ContentType { get; set; }
    }
}
