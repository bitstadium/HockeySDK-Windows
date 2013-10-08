using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HockeyApp.Model
{
    [DataContract]
    public class FeedbackThread
    {
        [DataMember]
        public string name { get; protected set; }
        [DataMember]
        public string email { get; protected set; }
        [DataMember]
        public int id { get; protected set; }
        [DataMember]
        public string created_at { get; protected set; } //TODO datetime conversion
        [DataMember]
        public string token { get; protected set; }

        [DataMember]
        public int status { get; protected set; }

        [DataMember]
        public List<FeedbackMessage> messages { get; protected set; }

    }
}
