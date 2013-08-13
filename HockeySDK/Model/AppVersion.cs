using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using HockeyApp.Tools;

namespace HockeyApp.Model
{
    [DataContract]
    public class AppVersion
    {

        public static IEnumerable<AppVersion> FromJson(Stream jsonStream) {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(AppVersion[]));
            return serializer.ReadObject(jsonStream) as IEnumerable<AppVersion>;
        }

        [DataMember]
        public string version { get; set; }
        [DataMember]
        public string shortversion { get; set; }
        [DataMember]
        public string title { get; set; }
        [DataMember]
        public Int64? timestamp { get; set; }
        [DataMember]
        public Int64 appsize { get; set; }
        [DataMember]
        public string notes { get; set; }
        [DataMember]
        public bool mandatory { get; set; }
        [DataMember]
        public string minimum_os_version { get; set; }
        [DataMember]
        public string device_family { get; set; }
        [DataMember]
        public int id { get; set; }
        [DataMember]
        public string app_id { get; set; }

        public string PublicIdentifier { get; set; }

        public DateTime? TimeStamp
        {
            get
            {
                return timestamp.UnixTimeStampToDateTime();
            }
        }

        public string AppSizeReadable
        {
            get
            {
                return appsize.ToReadableByteString();
            }
        }

        public string ShortversionAndVersion
        {
            get
            {
                if (version != null)
                {
                    if (shortversion != null)
                    {
                        return this.shortversion + " (" + this.version + ")";
                    }
                    else
                    {
                        return this.version;
                    }
                }
                else
                {
                    return (this.shortversion ?? "N/A") + " (" + (this.version ?? "N/A") + ")";
                }
            }
        }
    }
}
