using System.IO;
using Microsoft.HockeyApp.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.HockeyApp.Model
{
    /// <summary>
    /// represents an attachment to a feedback message
    /// </summary>
    [DataContract]
    public class FeedbackAttachment : IFeedbackAttachment
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="dataBytes"></param>
        /// <param name="contentType"></param>
        public FeedbackAttachment(string fileName, byte[] dataBytes, string contentType)
        {
            this.FileName = fileName;
            this.ContentType = contentType;
            this.DataBytes = dataBytes;
        }

        /// <summary>
        /// Remote URL where this attachment is available
        /// </summary>
        [DataMember(Name = "url")]
        public string RemoteURL { get; internal set; }
        /// <summary>
        /// Timestamp of creation
        /// </summary>
        [DataMember(Name = "created_at")]
        public string CreatedAt { get; internal set; }
        /// <summary>
        /// unique Id of the attachment
        /// </summary>
        [DataMember(Name = "id")]
        public string Id { get; internal set; }

        /// <summary>
        /// Name of the file when it was uploaded
        /// </summary>
        [DataMember(Name = "file_name")]
        public string FileName { get; set; }
        /// <summary>
        /// Bytes (usually only used when uploading attachments)
        /// </summary>
        public byte[] DataBytes { get; set; }
        /// <summary>
        /// Mime content type
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Load attachment to local storage
        /// </summary>
        /// <returns></returns>
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
