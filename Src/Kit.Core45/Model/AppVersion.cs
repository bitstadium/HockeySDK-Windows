using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.HockeyApp.Extensions;

namespace Microsoft.HockeyApp.Model
{
    /// <summary>
    /// represents a version from an hockeyapp app
    /// </summary>
    [DataContract]
    public class AppVersion : IAppVersion
    {
        /// <summary>
        /// unmarshal a AppVersion from a jsonStream
        /// </summary>
        /// <param name="jsonStream">json to unmarshall</param>
        /// <returns>populated AppVersion</returns>
        public static IEnumerable<AppVersion> FromJson(Stream jsonStream) {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<AppVersion>));
            return serializer.ReadObject(jsonStream) as IEnumerable<AppVersion>;
        }

        /// <summary>
        /// unique public identifier of the app
        /// </summary>
        public string PublicIdentifier { get; internal set; }

        /// <summary>
        /// version string
        /// </summary>
        [DataMember(Name = "version")]
        public string Version { get; set; }

        /// <summary>
        /// short version string
        /// </summary>
        [DataMember(Name = "shortversion")]
        public string Shortversion { get; set; }

        /// <summary>
        /// app title
        /// </summary>
        [DataMember(Name = "title")]
        public string Title { get; set; }

        /// <summary>
        /// app version timestamp
        /// </summary>
        [DataMember(Name = "timestamp")]
        public Int64? Timestamp { get; set; }

        /// <summary>
        /// size of version binary
        /// </summary>
        [DataMember(Name = "appsize")]
        public Int64 Appsize { get; set; }

        /// <summary>
        /// app version notes
        /// </summary>
        [DataMember(Name = "notes")]
        public string Notes { get; set; }

        /// <summary>
        /// flag to indicate mandatory update
        /// </summary>
        [DataMember(Name = "mandatory")]
        public bool Mandatory { get; set; }

        /// <summary>
        /// minimum os version for this app version
        /// </summary>
        [DataMember(Name = "minimum_os_version")]
        public string MinimumOsVersion { get; set; }

        /// <summary>
        /// target device family
        /// </summary>
        [DataMember(Name = "device_family")]
        public string DeviceFamily { get; set; }

        /// <summary>
        /// version id
        /// </summary>
        [DataMember(Name = "id")]
        public int Id { get; set; }

        /// <summary>
        /// internal app id
        /// </summary>
        [DataMember(Name = "app_id")]
        public string AppId { get; set; }

        /// <summary>
        /// version timestamp as TimeStamp
        /// </summary>
        public DateTime? TimeStamp
        {
            get
            {
                return Timestamp.UnixTimeStampToDateTime();
            }
        }
        
        
        /// <summary>
        /// readable string of app size
        /// </summary>
        public string AppSizeReadable
        {
            get
            {
                return Appsize.ToReadableByteString();
            }
        }

        /// <summary>
        /// combined string with version and shortversion
        /// </summary>
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
