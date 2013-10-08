using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using HockeyApp.Extensions;
using System.IO;

namespace HockeyApp.Model
{
    [DataContract]
    public class FeedbackThread : IFeedbackThread
    {
        
        private static ILog _logger = HockeyLogManager.GetLog(typeof(FeedbackThread));
        public static FeedbackThread CreateInstance()
        {
            return new FeedbackThread() { Token = Guid.NewGuid().ToString(), IsNewThread = true, messages = new List<FeedbackMessage>() };
        }

        private FeedbackThread() { }

        public bool IsNewThread { get; private set; }

        [DataMember(Name="name")]
        public string Name { get; private set; }
        [DataMember(Name="email")]
        public string EMail { get; private set; }
        [DataMember(Name="id")]
        public int Id { get; private set; }
        [DataMember(Name="created_at")]
        public string CreatedAt { get; private set; } //TODO datetime conversion
        [DataMember(Name="token")]
        public string Token { get; private set; }

        [DataMember(Name="status")]
        public int Status { get; private set; }


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


        /// <summary>
        /// 
        /// </summary>
        /// <returns>FeedbackThread or null if the thread got deleted</returns>
        /// <exception cref="ApplicationException"></exception>
        internal static async Task<FeedbackThread> OpenFeedbackThreadAsync(HockeyClient client, string threadToken)
        {
            FeedbackThread retVal = null;
            _logger.Info("Try to get thread with ID={0}", new object[] { threadToken });

            var request = WebRequest.CreateHttp(new Uri(client.ApiBase + "apps/" + client.AppIdentifier + "/feedback/" + threadToken + ".json", UriKind.Absolute));
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
                    if (String.IsNullOrWhiteSpace(webex.Response.ContentType))
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


        public async Task<IFeedbackMessage> PostFeedbackMessage(string message, string email="", string subject="")
        {
            if (String.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("Parameter message must not be empty!");
            }

            FeedbackMessage msg = new FeedbackMessage();
            msg.Name = HockeyClient.Instance.UserID;
            msg.Text = message;
            msg.Email = email;
            msg.Subject = subject;

            IHockeyClient client = HockeyClient.Instance;
            WebRequest request = null;
            if (this.IsNewThread)
            {
                msg.Token = this.Token;
                request = WebRequest.CreateHttp(new Uri(client.ApiBase + "apps/" + client.AppIdentifier + "/feedback", UriKind.Absolute));
                request.Method = "Post";
            }
            else
            {
                request = WebRequest.CreateHttp(new Uri(client.ApiBase + "apps/" + client.AppIdentifier + "/feedback/" + this.Token + "/", UriKind.Absolute));
                request.Method = "Put";
            }
            
            request.SetHeader(HttpRequestHeader.UserAgent.ToString(), client.UserAgentString);


            string data = msg.SerializeToWwwForm();

            byte[] dataStream = Encoding.UTF8.GetBytes(data);
            request.ContentType = "application/x-www-form-urlencoded";
            request.SetHeader(HttpRequestHeader.ContentEncoding.ToString(), Encoding.UTF8.WebName.ToString());
            Stream stream = await request.GetRequestStreamAsync();
            stream.Write(dataStream, 0, dataStream.Length);
            stream.Dispose();

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
