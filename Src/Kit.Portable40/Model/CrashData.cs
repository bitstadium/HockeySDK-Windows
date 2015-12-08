using System;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Microsoft.HockeyApp.Extensions;
using System.Runtime.Serialization.Json;
using Microsoft.HockeyApp.Exceptions;
using System.Runtime.CompilerServices;

// Need to provide InternalsVisibleTo System.Runtime.Serialization to allow serialization of internal DataContracts/DataMembers in partial trust.
// In Silverlight you cannot use Reflection to call a non-public setter on a class unless you are in the scope of the class. 
// This has a side-effect in serialization because it requires having public setters for all properties that must be serialized. Well that is not really the case. You can make them internal and use the InternalsVisibleTo attribute to expose those internals to the Microsoft serialization assemblies like this
[assembly: InternalsVisibleTo("System.Runtime.Serialization" + AssemblyInfo.SystemRuntimeSerializationPublicKey)]
[assembly: InternalsVisibleTo("System.Runtime.Serialization.Json" + AssemblyInfo.SystemRuntimeSerializationPublicKey)]

namespace Microsoft.HockeyApp.Model
{
    /// <summary>
    /// represents data of a crashlog
    /// </summary>
    [DataContract]
    public class CrashData : ICrashData
    {
        private ILog _logger = HockeyLogManager.GetLog(typeof(CrashData));
        private HockeyClient _hockeyClient = null;
        internal CrashData(HockeyClient hockeyClient, Exception ex, CrashLogInformation crashLogInfo){
            if (hockeyClient == null) { throw new ArgumentNullException("hockeyClient"); }
            
            this._hockeyClient = hockeyClient;

            StringBuilder builder = new StringBuilder();
            builder.Append(crashLogInfo.ToString());
            builder.AppendLine();
            builder.Append(ex.StackTraceToString());
            this.Log = builder.ToString();

            this.UserID = this._hockeyClient.UserID;
            this.Contact = this._hockeyClient.ContactInformation;
            this.SDKName = this._hockeyClient.SdkName;
            this.SDKVersion = this._hockeyClient.SdkVersion;
            if (this._hockeyClient.DescriptionLoader != null)
            {
                try
                {
                    this.Description = this._hockeyClient.DescriptionLoader(ex);
                }
                catch (Exception e) {
                    hockeyClient.HandleInternalUnhandledException(e);
                }
            }
        }

        //needed for crashes from unity-log
        internal CrashData(HockeyClient hockeyClient, string logString, string stackTrace, CrashLogInformation crashLogInfo)
        {
            this._hockeyClient = hockeyClient;
            StringBuilder builder = new StringBuilder();
            builder.Append(crashLogInfo.ToString());
            builder.AppendLine();
            builder.Append(logString);
            builder.AppendLine();
            builder.AppendLine(string.IsNullOrEmpty(stackTrace) ? "  at unknown location" : stackTrace);
            this.Log = builder.ToString();

            this.UserID = this._hockeyClient.UserID;
            this.Contact = this._hockeyClient.ContactInformation;
            this.SDKName = this._hockeyClient.SdkName;
            this.SDKVersion = this._hockeyClient.SdkVersion;
            //we don't support DescriptionLoader from unity at the moment
        }

        /// <summary>
        /// called on deserializing
        /// </summary>
        /// <param name="context">context of (de)serializer</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2238:ImplementSerializationMethodsCorrectly")]
        [OnDeserializing]
        public void OnDeserializing(StreamingContext context)
        {
            this._hockeyClient = HockeyClient.Current as HockeyClient;
            _logger = HockeyLogManager.GetLog(typeof(CrashData));
        }

        /// <summary>
        /// log string
        /// </summary>
        [DataMember(Name = "log")]
        public string Log{get;set;}

        /// <summary>
        /// description string
        /// </summary>
        [DataMember(Name = "description")]
        public string Description { get; set; }
        
        /// <summary>
        /// user id 
        /// </summary>
        [DataMember(Name = "userID")]
        public string UserID { get; set; }

        /// <summary>
        /// contact info
        /// </summary>
        [DataMember(Name = "contact")]
        public string Contact{get;set;}

        /// <summary>
        /// sdk name
        /// </summary>
        [DataMember(Name = "sdk")]
        public string SDKName{get;set;}
        /// <summary>
        /// sdk version
        /// </summary>
        [DataMember(Name = "sdk_version")]
        public string SDKVersion{get;set;}

        /// <summary>
        /// sends the crashlog data to the hockeyapp serer
        /// </summary>
        /// <returns></returns>
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
            HttpWebRequest request = WebRequest.CreateHttp(new Uri(this._hockeyClient.ApiBaseVersion2 + "apps/" + this._hockeyClient.AppIdentifier + "/crashes"));

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            if (!String.IsNullOrWhiteSpace(this._hockeyClient.UserAgentString))
            {
                request.SetHeader(HttpRequestHeader.UserAgent.ToString(), this._hockeyClient.UserAgentString);
            }
            using (Stream stream = await request.GetRequestStreamAsync())
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(rawData);
                stream.Write(byteArray, 0, rawData.Length);
                stream.Flush();
            }

            try
            {
                using (WebResponse response = await request.GetResponseAsync()) { }
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

        /// <summary>
        /// serialize data to a stream
        /// </summary>
        /// <param name="outputStream"></param>
        public void Serialize(Stream outputStream)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(CrashData));
            serializer.WriteObject(outputStream, this);
        }

        /// <summary>
        /// unmarshall data from a stream
        /// </summary>
        /// <param name="inputStream">json data stream</param>
        /// <returns>populated CrashData</returns>
        public static CrashData Deserialize(Stream inputStream)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(CrashData));
            CrashData cd = serializer.ReadObject(inputStream) as CrashData;
            return cd;
        }
       
    }
}
