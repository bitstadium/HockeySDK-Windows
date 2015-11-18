using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.HockeyApp.Extensions;
using System.IO;
using Microsoft.HockeyApp.Internal;

namespace Microsoft.HockeyApp.Model
{
    /// <summary>
    /// represents a feedback thread on hockeyapp with one or more messages
    /// </summary>
    [DataContract]
    public class FeedbackThread : IFeedbackThread
    {
        
        private static ILog _logger = HockeyLogManager.GetLog(typeof(FeedbackThread));
        /// <summary>
        /// Creates a new thread.
        /// </summary>
        /// <returns>new thread</returns>
        public static IFeedbackThread CreateInstance()
        {
            return new FeedbackThread() { Token = Guid.NewGuid().ToString(), IsNewThread = true, messages = new List<FeedbackMessage>() };
        }

        private FeedbackThread() { }

        /// <summary>
        /// indicates if this thread was new (not on server yet)
        /// </summary>
        public bool IsNewThread { get; private set; }

        /// <summary>
        /// name of the thread
        /// </summary>
        [DataMember(Name="name")]
        public string Name { get; private set; }
        /// <summary>
        /// email of thread starter
        /// </summary>
        [DataMember(Name="email")]
        public string EMail { get; private set; }
        /// <summary>
        /// unique id
        /// </summary>
        [DataMember(Name="id")]
        public int Id { get; private set; }
        /// <summary>
        /// time of creation as string
        /// </summary>
        [DataMember(Name="created_at")]
        public string CreatedAt { get; private set; }
        /// <summary>
        /// unique token for this thread
        /// </summary>
        [DataMember(Name="token")]
        public string Token { get; private set; }

        /// <summary>
        /// status
        /// </summary>
        [DataMember(Name="status")]
        public int Status { get; private set; }


        /// <summary>
        /// the messages in this thread (newest message last)
        /// </summary>
        public List<IFeedbackMessage> Messages
        {
            get
            {
                List<IFeedbackMessage> lst = null;
                if (this.messages != null)
                {
                    lst = this.messages.ToList<IFeedbackMessage>();
                }
                return lst;
            }
        }

        [DataMember(Name = "messages")]
        internal List<FeedbackMessage> messages { get; set; }

        internal static async Task<FeedbackThread> OpenFeedbackThreadAsync(HockeyClient client, string threadToken)
        {
            FeedbackThread retVal = null;
            _logger.Info("Try to get thread with ID={0}", new object[] { threadToken });

            var request = WebRequest.CreateHttp(new Uri(client.ApiBaseVersion2 + "apps/" + client.AppIdentifier + "/feedback/" + threadToken + ".json", UriKind.Absolute));
            request.Method = "Get";
            request.SetHeader(HttpRequestHeader.UserAgent.ToString(), client.UserAgentString);

            try
            {
                var response = await request.GetResponseAsync();

                var fbResp = await TaskEx.Run<FeedbackResponseSingle>(() => FeedbackResponseSingle.FromJson(response.GetResponseStream()));

                if (fbResp.Status.Equals("success"))
                {
                    retVal = fbResp.Feedback;
                }
                else
                {
                    throw new Exception("Server error. Server returned status " + fbResp.Status);
                }
            }
            catch (Exception e)
            {
                var webex = e as WebException;
                if (webex != null)
                {
                    if (webex.Response == null || String.IsNullOrWhiteSpace(webex.Response.ContentType))
                    {
                        //Connection error during call
                        throw webex;
                    }
                    else
                    {
                        //404 Response from server => thread got deleted
                        retVal = null;
                    }
                }
                else
                {
                    throw;
                }
            }
            return retVal;
        }

        /// <summary>
        /// Posts the feedback message asynchronous.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="email">The email.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="name">The name.</param>
        /// <param name="attachments">The attachments.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Server error. Server returned status  + fbResp.Status</exception>
        public async Task<IFeedbackMessage> PostFeedbackMessageAsync(string message, string email = null, string subject = null, string name = null, IEnumerable<IFeedbackAttachment> attachments = null)
        {
            IHockeyClientInternal client = HockeyClient.Current.AsInternal();

            var msg = new FeedbackMessage();
            msg.Name = client.UserID;
            msg.Text = message;
            msg.Email = email;
            msg.Name = name;
            msg.Subject = subject;
            
            HttpWebRequest request = null;
            if (this.IsNewThread)
            {
                msg.Token = this.Token;
                request = WebRequest.CreateHttp(new Uri(client.ApiBaseVersion2 + "apps/" + client.AppIdentifier + "/feedback", UriKind.Absolute));
                request.Method = "Post";
            }
            else
            {
                request = WebRequest.CreateHttp(new Uri(client.ApiBaseVersion2 + "apps/" + client.AppIdentifier + "/feedback/" + this.Token + "/", UriKind.Absolute));
                request.Method = "Put";
            }
            
            request.SetHeader(HttpRequestHeader.UserAgent.ToString(), client.UserAgentString);

            byte[] dataStream;

            if (attachments == null || !attachments.Any())
            {
                string data = msg.SerializeToWwwForm();
                dataStream = Encoding.UTF8.GetBytes(data);
                request.ContentType = "application/x-www-form-urlencoded";
                request.SetHeader(HttpRequestHeader.ContentEncoding.ToString(), Encoding.UTF8.WebName.ToString());

                using (Stream stream = await request.GetRequestStreamAsync())
                {
                    stream.Write(dataStream, 0, dataStream.Length);
                    stream.Flush(); 
                }
            }
            else
            {
                string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
                byte[] boundarybytes = System.Text.Encoding.UTF8.GetBytes("\r\n--" + boundary + "\r\n");

                request.ContentType = "multipart/form-data; boundary=" + boundary;
                using (Stream stream = await request.GetRequestStreamAsync())
                {
                    string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";

                    //write form fields
                    foreach (var keyValue in msg.MessagePartsDict)
                    {
                        stream.Write(boundarybytes, 0, boundarybytes.Length);
                        string formitem = string.Format(formdataTemplate, keyValue.Key, keyValue.Value);
                        byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                        stream.Write(formitembytes, 0, formitembytes.Length);
                    }
                    //write images
                    for (int index = 0; index < attachments.Count(); index++)
                    {
                        var attachment = attachments.ElementAt(index);
                        stream.Write(boundarybytes, 0, boundarybytes.Length);
                        string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
                        string header = string.Format(headerTemplate, "attachment" + index, attachment.FileName, String.IsNullOrEmpty(attachment.ContentType) ? "application/octet-stream" : attachment.ContentType);
                        byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
                        stream.Write(headerbytes, 0, headerbytes.Length);
                        stream.Write(attachment.DataBytes, 0, attachment.DataBytes.Length);
                    }

                    byte[] trailer = System.Text.Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");
                    stream.Write(trailer, 0, trailer.Length);
                    stream.Flush(); 
                }
            }

            var response = await request.GetResponseAsync();
            var fbResp = await TaskEx.Run<FeedbackResponseSingle>(() => FeedbackResponseSingle.FromJson(response.GetResponseStream()));

            if (!fbResp.Status.Equals("success"))
            {
                throw new Exception("Server error. Server returned status " + fbResp.Status);
            }

            IFeedbackMessage fbNewMessage = fbResp.Feedback.Messages.Last();

            if (fbNewMessage != null)
            {
                this.messages.Add(fbNewMessage as FeedbackMessage);
            }

            if (this.IsNewThread)
            {
                this.IsNewThread = false;
            }
            return fbNewMessage;
        }
    }
}
