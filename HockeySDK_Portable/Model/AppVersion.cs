using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using HockeyApp.Extensions;

namespace HockeyApp.Model
{
    [DataContract]
    public class AppVersion : IAppVersion
    {
        
        public static IEnumerable<AppVersion> FromJson(Stream jsonStream) {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<AppVersion>));
            return serializer.ReadObject(jsonStream) as IEnumerable<AppVersion>;
        }

        public string PublicIdentifier { get; internal set; }

        [DataMember(Name = "version")]
        public string Version { get; set; }

        [DataMember(Name = "shortversion")]
        public string Shortversion { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "timestamp")]
        public Int64? Timestamp { get; set; }

        [DataMember(Name = "appsize")]
        public Int64 Appsize { get; set; }

        [DataMember(Name = "notes")]
        public string Notes { get; set; }

        [DataMember(Name = "mandatory")]
        public bool Mandatory { get; set; }

        [DataMember(Name = "minimum_os_version")]
        public string MinimumOsVersion { get; set; }

        [DataMember(Name = "device_family")]
        public string DeviceFamily { get; set; }

        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "app_id")]
        public string AppId { get; set; }

        public DateTime? TimeStamp
        {
            get
            {
                return Timestamp.UnixTimeStampToDateTime();
            }
        }

        public string AppSizeReadable
        {
            get
            {
                return Appsize.ToReadableByteString();
            }
        }

        public string ShortversionAndVersion
        {
            get
            {
                if (Version != null)
                {
                    if (Shortversion != null)
                    {
                        return this.Shortversion + " (" + this.Version + ")";
                    }
                    else
                    {
                        return this.Version;
                    }
                }
                else
                {
                    return (this.Shortversion ?? "N/A") + " (" + (this.Version ?? "N/A") + ")";
                }
            }
        }
    }
}
