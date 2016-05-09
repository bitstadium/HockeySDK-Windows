using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.HockeyApp.Extensions;
using System.Diagnostics;

namespace Microsoft.HockeyApp.Model
{
    /// <summary>
    /// representation of a feedback response with a single message (returned after posting the message)
    /// </summary>
    [DataContract]
    public class FeedbackResponseSingle
    {
        private static ILog _log = HockeyLogManager.GetLog(typeof(FeedbackResponseSingle));
        /// <summary>
        /// unmarshal from json stream.
        /// </summary>
        /// <param name="jsonStream">The json stream.</param>
        /// <returns>populated object</returns>
        public static FeedbackResponseSingle FromJson(Stream jsonStream)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(FeedbackResponseSingle));
            return serializer.ReadObject(jsonStream) as FeedbackResponseSingle;
        }

        /// <summary>
        /// Gets the feedback thread.
        /// </summary>
        [DataMember(Name="feedback")]
        public FeedbackThread Feedback { get; private set; }

        /// <summary>
        /// Gets the feedback token.
        /// </summary>
        [DataMember(Name="token")]
        public string FeedbackToken { get; private set; }

        /// <summary>
        /// Gets the status of the response (success)
        /// </summary>
        [DataMember(Name="status")]
        public string Status { get; private set; }
       
    }

}
