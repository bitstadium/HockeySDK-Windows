using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using HockeyApp.Extensions;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Json;
using HockeyApp.Exceptions;

namespace HockeyApp.Model
{
    internal class AuthType
    {
        internal string StatusCode { get; private set; }
        internal string IdCode { get; private set; }
        internal bool IsAuthorized { get; private set; }
        internal bool IsIdentified { get; private set; }
        internal bool IsValid { get; private set; }
        internal Func<AuthStatus,string> IdFunc { get; set;}

        internal AuthType(string statusCode, string idCode, bool isAuthorized, bool isIdentified, bool isValid, Func<AuthStatus,string> idFunc) 
        {
            StatusCode = statusCode;
            IdCode = idCode;
            IsAuthorized = isAuthorized;
            IsIdentified = isIdentified;
            IsValid = IsValid;
            IdFunc = idFunc;
        }

        internal static AuthType Identified = new AuthType(statusCode: "identified", idCode: "iuid", isAuthorized: false, isIdentified: true, isValid: true, idFunc: a => a.Iuid);
        internal static AuthType Authorized = new AuthType(statusCode: "authorized", idCode: "auid", isAuthorized: true, isIdentified: true, isValid: true, idFunc: a => a.Iuid);
        internal static AuthType Validated = new AuthType(statusCode: "validated", idCode: "", isAuthorized: false, isIdentified: true, isValid: true, idFunc: a => "");
        internal static AuthType Invalid = new AuthType(statusCode: "invalid", idCode: "", isAuthorized: false, isIdentified: false, isValid: false, idFunc: a => "");
        internal static AuthType NotAuthorized = new AuthType(statusCode: "not authorized", idCode: "", isAuthorized: false, isIdentified: false, isValid: false, idFunc: a => "");
        internal static AuthType NotFound = new AuthType(statusCode: "not found", idCode: "", isAuthorized: false, isIdentified: false, isValid: false, idFunc: a => "");
        
        static internal IEnumerable<AuthType> AuthTypes { get { return new List<AuthType>() { Authorized, Identified, Invalid }; } }
    }

    [DataContract]
    public sealed class AuthStatus : IAuthStatus
    {
        private AuthType authType;
        internal AuthType AuthType
        {
            get { return authType ?? AuthType.Invalid; }
            private set { authType = value; }
        }

        private AuthStatus() { }

        [DataMember] private String status { get; set; }
        [DataMember] internal String Iuid { get; private set; }
        [DataMember] internal String Auid { get; private set; }

        internal static AuthStatus InvalidAuthStatus { get { return new AuthStatus() { AuthType = AuthType.Invalid }; } }

        public string SerializeToString()
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(AuthStatus));
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.WriteObject(ms, this);
                var bytes = ms.ToArray();
                return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            }
        }

        public static IAuthStatus DeserializeFromString(String aSerializedAuthStatus)
        {
            return FromJson(new MemoryStream(Encoding.UTF8.GetBytes(aSerializedAuthStatus)));
        }

        internal static IAuthStatus FromJson(Stream jsonStream)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(AuthStatus));
            var auth = serializer.ReadObject(jsonStream) as AuthStatus;
            var atype = AuthType.AuthTypes.FirstOrDefault(t => t.StatusCode.Equals(auth.status));
            auth.AuthType = atype;
            return auth;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CheckIfStillValid()
        {
            var request = WebRequest.CreateHttp(new Uri(HockeyClient.Instance.ApiBaseVersion2 + "apps/" + 
                                                            HockeyClient.Instance.AppIdentifier + "/identity/validate?" + 
                                                                this.AuthType.IdCode + "=" + this.AuthType.IdFunc(this) , UriKind.Absolute));
            request.Method = "Get";
            request.SetHeader(HttpRequestHeader.UserAgent.ToString(), HockeyClient.Instance.UserAgentString);
            return (await DoAuthRequestHandleResponse(request)).IsIdentified;
        }

        internal static async Task<IAuthStatus> DoAuthRequestHandleResponse(HttpWebRequest request)
        {
            WebResponse response = null;
            try
            {
                response = await request.GetResponseAsync();
            }
            catch (WebException e)
            {
                if ((e.Status == WebExceptionStatus.ConnectFailure) ||
                    (e.Status == WebExceptionStatus.SendFailure))
                {
                    throw new WebTransferException("Could not connect to server.", e);
                }
                else
                {
                    if ((e.Response as HttpWebResponse).StatusCode.Equals(HttpStatusCode.NotFound)
                        || (e.Response as HttpWebResponse).StatusCode.Equals(HttpStatusCode.Unauthorized))
                    {
                        return AuthStatus.InvalidAuthStatus;
                    }
                }
            }
            IAuthStatus checkedAuthStatus = await TaskEx.Run(() => AuthStatus.FromJson(response.GetResponseStream()));
            return checkedAuthStatus;
        }
        public bool IsAuthorized { get { return this.AuthType.IsAuthorized; } }
        public bool IsIdentified { get { return this.AuthType.IsIdentified; } }
    }

}
