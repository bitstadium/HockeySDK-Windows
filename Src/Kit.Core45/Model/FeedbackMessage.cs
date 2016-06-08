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
    /// represents a feedback message
    /// </summary>
    [DataContract]
    public class FeedbackMessage : IFeedbackMessage
    {
        /// <summary>
        /// types of message sources
        /// </summary>
        public enum ViaTypes
        {
            /// <summary>
            /// via rest api (normally by an app using the sdk)
            /// </summary>
            API = 1,
            /// <summary>
            /// via email
            /// </summary>
            Email = 2,
            /// <summary>
            /// via web interface
            /// </summary>
            Web = 3
        }

        /// <summary>
        /// message attachments
        /// </summary>
        [DataMember(Name="name")]
        public string Name { get; set; }

        /// <summary>
        /// email of the message sender
        /// </summary>
        [DataMember(Name="email")]
        public string Email { get; set; }

        /// <summary>
        /// Gravatar hash of the senders email
        /// </summary>
        [DataMember(Name = "gravatar_hash")]
        public string GravatarHash { get; set; }

        /// <summary>
        /// subject
        /// </summary>
        [DataMember(Name = "subject")]
        public string Subject { get; set; }

        /// <summary>
        /// text (can include html)
        /// </summary>
        [DataMember(Name = "Text")]
        public string Text { get; set; }

        /// <summary>
        /// Device oem
        /// </summary>
        [DataMember(Name = "oem")]
        public string Oem { get; protected set; }

        /// <summary>
        /// Device model
        /// </summary>
        [DataMember(Name = "Model")]
        public string Model { get; protected set; }

        /// <summary>
        /// corresponding OS Version
        /// </summary>
        [DataMember(Name = "os_version")]
        public string OSVersion { get; protected set; }

        /// <summary>
        /// Timestamp of creation as string
        /// </summary>
        [DataMember(Name = "created_at")]
        public string CreatedAt { get; protected set; }

        [DataMember(Name = "attachments")]
        internal List<FeedbackAttachment> attachments { get; set; }

        /// <summary>
        /// message attachments
        /// </summary>
        public IEnumerable<IFeedbackAttachment> Attachments
        {
            get
            {
                return this.attachments != null ? this.attachments.Cast<IFeedbackAttachment>() : new List<IFeedbackAttachment>();
            }
        }

        /// <summary>
        /// Timestamp of creation
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.DateTime.Parse(System.String)")]
        public DateTime Created
        {
            get
            {
                return String.IsNullOrWhiteSpace(CreatedAt) ?  DateTime.Now : DateTime.Parse(CreatedAt);
            }
        }

        /// <summary>
        /// unique id of the message
        /// </summary>
        [DataMember(Name = "id")]
        public int Id { get; private set; }

        /// <summary>
        /// Token for message
        /// </summary>
        [DataMember(Name = "token")]
        public string Token { get; internal set; }
        /// <summary>
        /// via-flag (indicates if from web/api ..)
        /// </summary>
        [DataMember(Name = "via")]
        public int Via { get; private set; }

        /// <summary>
        /// User
        /// </summary>
        [DataMember(Name = "user_string")]
        public string UserString { get; protected set; }

        /// <summary>
        /// Message is only internal
        /// </summary>
        [DataMember(Name = "internal")]
        public bool? @Internal { get; private set; }

        /// <summary>
        /// Raw text of the message
        /// </summary>
        [DataMember(Name = "clean_text")]
        public string CleanText { get; private set; }
        
        /// <summary>
        /// App id of the app this feedback message belongs to
        /// </summary>
        [DataMember(Name = "app_id")]
        public string AppId { get; private set; }

        /// <summary>
        /// App version where the feedback relates to
        /// </summary>
        [DataMember(Name = "app_verson_id")]
        public string AppVersionId { get; private set; }

        /// <summary>
        /// via-flag as string
        /// </summary>
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
                if (!String.IsNullOrWhiteSpace(HockeyClient.Current.AsInternal().VersionInfo)) { partsDict.Add("bundle_version", HockeyClient.Current.AsInternal().VersionInfo); }
                return partsDict;
            }
        }

        internal string SerializeToWwwForm()
        {
            return MessagePartsDict.Select(e => e.Key + "=" + Uri.EscapeUriString(e.Value)).Aggregate((a, b) => a + "&" + b);
        }
    }
}
