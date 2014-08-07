using System.IO;
using HockeyApp.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HockeyApp.Model
{
    [DataContract]
    public class FeedbackAttachment : IFeedbackAttachment
    {
        public FeedbackAttachment(string fileName, byte[] dataBytes, string contentType)
        {
            this.FileName = fileName;
            this.ContentType = contentType;
            this.DataBytes = dataBytes;
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

        public async Task<bool> LoadAttachmentFromServer()
        {
            bool retVal = false;
            if (String.IsNullOrWhiteSpace(this.RemoteURL))
            {
                throw new Exception("Attachment not found! Did you upload your attachment to the server?");
            }

            var request = WebRequest.CreateHttp(new Uri(this.RemoteURL, UriKind.Absolute));
            request.Method = "Get";
            request.SetHeader(HttpRequestHeader.UserAgent.ToString(), HockeyClient.Current.AsInternal().UserAgentString);

            try
            {
                var response = await request.GetResponseAsync();
                var ms = new MemoryStream();
                response.GetResponseStream().CopyTo(ms);
                this.DataBytes = ms.ToArray();
                retVal = true;
            }
            catch (Exception e)
            {
                var webex = e as WebException;
                if (webex != null)
                {
                    if (String.IsNullOrWhiteSpace(webex.Response.ContentType))
                    {
                        //Connection error during call
                        throw webex;
                    }
                    else
                    {
                        //404 Response from server => thread got deleted
                        retVal = false;
                    }
                }
                else
                {
                    throw;
                }
            }
            return retVal;

        }
    }
}
