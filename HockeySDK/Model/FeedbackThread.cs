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
        public string name { get; set; }
        [DataMember]
        public string email { get; set; }
        [DataMember]
        public int id { get; set; }
        [DataMember]
        public string created_at { get; set; } //TODO datetime
        [DataMember]
        public string token { get; set; }

        [DataMember]
        public int status { get; set; }

        [DataMember]
        public List<FeedbackMessage> messages { get; set; }

    }
}
