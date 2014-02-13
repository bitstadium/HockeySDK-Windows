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
    public class FeedbackMessage : IFeedbackMessage
    {
        public enum ViaTypes
        {
            API = 1,
            Email = 2,
            Web = 3
        }

        [DataMember(Name="name")]
        public string Name { get; set; }

        [DataMember(Name="email")]
        public string Email { get; set; }

        [DataMember(Name = "gravatar_hash")]
        public string GravatarHash { get; set; }

        [DataMember(Name = "subject")]
        public string Subject { get; set; }

        [DataMember(Name = "Text")]
        public string Text { get; set; }

        [DataMember(Name = "oem")]
        public string Oem { get; protected set; }

        [DataMember(Name = "Model")]
        public string Model { get; protected set; }

        [DataMember(Name = "os_version")]
        public string OSVersion { get; protected set; }

        [DataMember(Name = "created_at")]
        public string CreatedAt { get; protected set; }

        [DataMember(Name = "attachments")]
        internal List<FeedbackAttachment> attachments { get; set; }

        public IEnumerable<IFeedbackAttachment> Attachments
        {
            get
            {
                return this.attachments != null ? this.attachments.Cast<IFeedbackAttachment>() : new List<IFeedbackAttachment>();
            }
        }
        
        public DateTime Created
        {
            get
            {
                return String.IsNullOrWhiteSpace(CreatedAt) ?  DateTime.Now : DateTime.Parse(CreatedAt);
            }
        }

        [DataMember(Name = "id")]
        public int Id { get; private set; }

        [DataMember(Name = "token")]
        public string Token { get; internal set; }

        [DataMember(Name = "via")]
        public int Via { get; private set; }

        [DataMember(Name = "user_string")]
        public string UserString { get; protected set; }

        [DataMember(Name = "internal")]
        public bool? @Internal { get; private set; }

        [DataMember(Name = "clean_text")]
        public string CleanText { get; private set; }

        [DataMember(Name = "app_id")]
        public string AppId { get; private set; }

        [DataMember(Name = "app_verson_id")]
        public string AppVersionId { get; private set; }

        public string ViaAsString { get {
            String retVal = "";
            switch (this.Via)
            {
                case 1:
                    retVal = "API";
                    break;
                case 2:
                    retVal = "E-Mail";
                    break;
                case 3:
                    retVal = "Web";
                    break;
            }
            return retVal;
        } }

        internal Dictionary<string, string> MessagePartsDict
        {
            get
            {
                var partsDict = new Dictionary<string, string>();
                if (!String.IsNullOrWhiteSpace(this.Text)) { partsDict.Add("text", this.Text.Replace("\r", "\n")); }
                if (!String.IsNullOrWhiteSpace(this.Name)) { partsDict.Add("name", this.Name); }
                if (!String.IsNullOrWhiteSpace(this.Email)) { partsDict.Add("email", this.Email); }
                if (!String.IsNullOrWhiteSpace(this.Subject)) { partsDict.Add("subject", this.Subject); }

                if (!String.IsNullOrWhiteSpace(this.Token)) { partsDict.Add("token", this.Token); }

                if (!String.IsNullOrWhiteSpace(Oem)) { partsDict.Add("oem", Oem); }
                if (!String.IsNullOrWhiteSpace(Model)) { partsDict.Add("model", Model); }
                if (!String.IsNullOrWhiteSpace(OSVersion)) { partsDict.Add("os_version", this.OSVersion); }

                //not used for feedback. if (HockeyClient.Instance.AppIdentifier != null) { partsDict.Add("bundle_identifier", HockeyClient.Instance.AppIdentifier); }
                if (!String.IsNullOrWhiteSpace(HockeyClient.Instance.VersionInfo)) { partsDict.Add("bundle_version", HockeyClient.Instance.VersionInfo); }
                return partsDict;
            }
        }

        internal string SerializeToWwwForm()
        {
            return MessagePartsDict.Select(e => e.Key + "=" + Uri.EscapeUriString(e.Value)).Aggregate((a, b) => a + "&" + b);
        }
    }
}
