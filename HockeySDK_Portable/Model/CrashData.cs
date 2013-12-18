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
using System.Runtime.Serialization.Json;
using HockeyApp.Exceptions;
using System.Runtime.CompilerServices;

//TODO make it work with InternalsVisibleTo (PublicKey ?!) and make class and OnDeserializing internal
[assembly: InternalsVisibleTo("System.Runtime.Serialization")]
[assembly: InternalsVisibleTo("System.Runtime.Serialization.Json")]

namespace HockeyApp.Model
{
    [DataContract]
    public class CrashData : ICrashData
    {
        private ILog _logger = HockeyLogManager.GetLog(typeof(CrashData));
        private HockeyClient _hockeyClient = null;
        internal CrashData(HockeyClient hockeyClient, Exception ex, string crashLogInfo){
            this._hockeyClient = hockeyClient;

            StringBuilder builder = new StringBuilder();
            builder.Append(crashLogInfo);
            builder.AppendLine();
            builder.Append(ex.StackTraceToString());
            this.Log = builder.ToString();

            this.UserID = this._hockeyClient.UserID;
            this.Contact = this._hockeyClient.ContactInformation;
            this.SDKName = this._hockeyClient.SdkName;
            this.SDKVersion = this._hockeyClient.SdkVersion;
            if (this._hockeyClient._descriptionLoader != null)
            {
                try
                {
                    this.Description = this._hockeyClient._descriptionLoader(ex);
                }
                catch (Exception) { }
            }
        }

        [OnDeserializing]
        public void OnDeserializing(StreamingContext context)
        {
            this._hockeyClient = HockeyClient.Instance as HockeyClient;
        }

        [DataMember(Name = "log")]
        public string Log{get;set;}

        [DataMember(Name = "description")]
        public string Description { get; set; }
        
        [DataMember(Name = "userID")]
        public string UserID { get; set; }

        [DataMember(Name = "contact")]
        public string Contact{get;set;}

        [DataMember(Name = "sdk")]
        public string SDKName{get;set;}
        
        [DataMember(Name = "sdk_version")]
        public string SDKVersion{get;set;}


        public async Task SendDataAsync()
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

            try
            {
                WebResponse response = await request.GetResponseAsync();
            }
            catch (WebException e)
            {
                _logger.Error(e);
                if ((e.Status == WebExceptionStatus.ConnectFailure) ||
                    (e.Status == WebExceptionStatus.SendFailure) ||
                    (e.Status == WebExceptionStatus.UnknownError))
                {
                    throw new WebTransferException("Transfer of Crashdata to server failed", e);
                }
            }
        }

        public void Serialize(Stream outputStream)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(CrashData));
            serializer.WriteObject(outputStream, this);
        }

        public static CrashData Deserialize(Stream inputStream)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(CrashData));
            CrashData cd = serializer.ReadObject(inputStream) as CrashData;
            return cd;
        }


       
    }
}
