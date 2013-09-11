using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using HockeyApp.Tools;

namespace HockeyApp.Model
{
    [DataContract]
    public class FeedbackMessage
    {
        public enum Via
        {
            API = 1,
            Email = 2,
            Web = 3
        }

        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string email { get; set; }
        [DataMember]
        public string subject { get; set; }
        [DataMember]
        public string text { get; set; }

        [DataMember]
        public string oem { get; protected set; }
        [DataMember]
        public string model { get; protected set; }
        [DataMember]
        public string os_version { get; protected set; }
        [DataMember]
        public string created_at { get; protected set; } //TODO datetime?!

        public DateTime Created
        {
            get
            {
                return created_at.IsEmpty() ?  DateTime.Now : DateTime.Parse(created_at);
            }
        }

        [DataMember]
        public int id { get; private set; }
        [DataMember]
        public string token { get; protected set; }
        [DataMember]
        public int via { get; private set; }
        [DataMember]
        public string user_string { get; protected set; }
        [DataMember]
        public bool? @internal { get; private set; }
        [DataMember]
        public string clean_text { get; private set; }
        [DataMember]
        public string app_id { get; private set; }
        [DataMember]
        public string app_version_id { get; private set; }

        public string bundle_short_version { get { return ManifestHelper.GetAppVersion(); } }
        public string bundle_identifier { get { return FeedbackManager.Instance.Application.GetType().Namespace; } }
        public string bundle_version { get { return ""; } } //TODO version oder shortverversion

        internal string SerializeToWwwForm()
        {
            var partsDict = new Dictionary<string, string>();
            partsDict.Add("text", this.text.Replace("\r","\n"));
            partsDict.Add("name", this.name);
            partsDict.Add("email", this.email);
            partsDict.Add("subject", this.subject);

            if (oem != null) { partsDict.Add("oem", oem); }
            if (model != null) { partsDict.Add("model", model); }
            if (os_version != null) { partsDict.Add("os_version", os_version); }
            if (bundle_identifier != null) { partsDict.Add("bundle_identifier", bundle_identifier); }
            if (bundle_version != null) { partsDict.Add("bundle_version", bundle_version); }
            if (bundle_short_version != null) { partsDict.Add("bundle_short_version", bundle_short_version); }
            return partsDict.Select(e => e.Key + "=" + HttpUtility.UrlEncode(e.Value)).Aggregate((a, b) => a + "&" + b);
        }
    }
}
