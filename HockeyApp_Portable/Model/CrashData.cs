using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Reflection;
using HockeyApp.Extensions;

namespace HockeyApp.Model
{
    [DataContract]
    internal class CrashData : ICrashData
    {
        private HockeyClient _hockeyClient = null;
        internal CrashData(HockeyClient hockeyClient){
            this._hockeyClient = hockeyClient;
            this.SDKName = this._hockeyClient.SdkName;
            this.SDKVersion = this._hockeyClient.SdkVersion;
        }

        /// <summary>
        /// required, file with the crash log - Note: The maximum allowed file size is 200kB!
        /// </summary>
        /// [DataMember(Name = "log")]
        public string Log{get;set;}

        /// <summary>
        /// optional, file with optional information, e.g. excerpts from the system log
        /// </summary>
        [DataMember(Name = "description")]
        public string Description { get; set; }
        
        /// <summary>
        /// optional, string with a user or device ID, limited to 255 chars
        /// </summary>
        [DataMember(Name = "userID")]
        public string UserID { get; set; }

        /// <summary>
        /// optional, string with contact information, limited to 255 chars
        /// </summary>
        [DataMember(Name = "contact")]
        public string Contact{get;set;}

        /// <summary>
        /// Name of the used SDK
        /// </summary>
        [DataMember(Name = "sdk")]
        public string SDKName{get;set;}
        
        /// <summary>
        /// Version of the used SDK
        /// </summary>
        [DataMember(Name = "sdk_version")]
        public string SDKVersion{get;set;}


        internal async Task SendData()
        {
            string rawData = "";
            rawData += "raw=" + Uri.EscapeDataString(this.Log);
            if (this.UserID != null)
            {
                rawData += "&userID=" + Uri.EscapeDataString(this.UserID);
            }
            if (this.Contact != null)
            {
                rawData += "&contact=" + Uri.EscapeDataString(this.Contact);
            }
            if (this.Description != null)
            {
                rawData += "&description=" + Uri.EscapeDataString(this.Description);
            }

            rawData += "&sdk=" + Uri.EscapeDataString(this.SDKName);
            rawData += "&sdk_version=" + Uri.EscapeDataString(this.SDKVersion);


            //Exception should be handled by caller
            HttpWebRequest request = WebRequest.CreateHttp(new Uri(this._hockeyClient.ApiBase + "apps/" + this._hockeyClient.AppIdentifier + "/crashes"));

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.SetHeader(HttpRequestHeader.UserAgent.ToString(), this._hockeyClient.UserAgentString);
            Stream stream = await request.GetRequestStreamAsync();
            byte[] byteArray = Encoding.UTF8.GetBytes(rawData);
            stream.Write(byteArray, 0, rawData.Length);
            stream.Dispose();
            WebResponse response = await request.GetResponseAsync();
        }

        
    }
}
