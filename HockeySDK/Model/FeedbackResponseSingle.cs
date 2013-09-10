using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace HockeyApp.Model
{
    [DataContract]
    public class FeedbackResponseSingle
    {

        public static FeedbackResponseSingle FromJson(Stream jsonStream)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(FeedbackResponseSingle));
            return serializer.ReadObject(jsonStream) as FeedbackResponseSingle;
        }

        [DataMember]
        public FeedbackThread feedback { get; set; }

        [DataMember]
        public string token { get; set; }
        [DataMember]
        public string status { get; set; }

    }
}
