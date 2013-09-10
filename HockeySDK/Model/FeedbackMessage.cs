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
    public class FeedbackMessage
    {
        [DataMember]
        public string subject { get; set; }
        [DataMember]
        public string text { get; set; }
        [DataMember]
        public string oem { get; set; }
        [DataMember]
        public string model { get; set; }

        [DataMember]
        public string os_version { get; set; }
        [DataMember]
        public string created_at { get; set; } //TODO datetime?!
        [DataMember]
        public int id { get; set; }
        [DataMember]
        public string token { get; set; }
        [DataMember]
        public int via { get; set; }
        [DataMember]
        public string user_string { get; set; }
        [DataMember]
        public string @internal { get; set; } //TODO int ?? name
        [DataMember]
        public string clean_text { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string email { get; set; }
        [DataMember]
        public string app_id { get; set; }
        [DataMember]
        public string app_version_id { get; set; }

        public string bundle_short_version { get { return ManifestHelper.GetAppVersion(); } }
        public string bundle_identifier { get { return "TODO"; } } //TODO aus der application klasse holen !?
        public string bundle_version { get { return ""; } }

        public string SerializeToWwwForm()
        {
            var partsDict = new Dictionary<string, string>();
            partsDict.Add("text", this.text);
            partsDict.Add("name", this.name);
            partsDict.Add("email", this.email);
            partsDict.Add("subject", this.subject);

            if (oem != null) { partsDict.Add("oem", subject); }
            if (model != null) { partsDict.Add("model", subject); }
            if (os_version != null) { partsDict.Add("os_version", subject); }
            if (bundle_identifier != null) { partsDict.Add("bundle_identifier", subject); }
            if (bundle_version != null) { partsDict.Add("bundle_version", subject); }
            if (bundle_short_version != null) { partsDict.Add("bundle_short_version", subject); }
            return partsDict.Select(e => e.Key + "=" + HttpUtility.UrlEncode(e.Value)).Aggregate((a, b) => a + "&" + b);
        }
    }
}
