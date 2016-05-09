using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.HockeyApp.Extensions;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Json;
using Microsoft.HockeyApp.Exceptions;

namespace Microsoft.HockeyApp.Model
{
    internal class AuthType
    {
        internal const string STATUS_IDENTIFIED = "identified";
        internal const string STATUS_AUTHORIZED = "authorized";
        internal const string STATUS_VALIDATED = "validated";
        internal const string STATUS_INVALID = "invalid";
        internal const string STATUS_NOT_AUTHORIZED = "not authorized";
        internal const string STATUS_NOT_FOUND = "not found";

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

        internal static AuthType Identified = new AuthType(statusCode: STATUS_IDENTIFIED, idCode: "iuid", isAuthorized: false, isIdentified: true, isValid: true, idFunc: a => a.Iuid);
        internal static AuthType Authorized = new AuthType(statusCode: STATUS_AUTHORIZED, idCode: "auid", isAuthorized: true, isIdentified: true, isValid: true, idFunc: a => a.Auid);
        internal static AuthType Validated = new AuthType(statusCode: STATUS_VALIDATED, idCode: "", isAuthorized: false, isIdentified: true, isValid: true, idFunc: a => "");
        internal static AuthType Invalid = new AuthType(statusCode: STATUS_INVALID, idCode: "", isAuthorized: false, isIdentified: false, isValid: false, idFunc: a => "");
        internal static AuthType NotAuthorized = new AuthType(statusCode: STATUS_NOT_AUTHORIZED, idCode: "", isAuthorized: false, isIdentified: false, isValid: false, idFunc: a => "");
        internal static AuthType NotFound = new AuthType(statusCode: STATUS_NOT_FOUND, idCode: "", isAuthorized: false, isIdentified: false, isValid: false, idFunc: a => "");

        static internal IEnumerable<AuthType> AuthTypes { get { return new List<AuthType>() { Authorized, Identified, Validated, NotFound, NotAuthorized, Invalid }; } }
    }

    /// <summary>
    /// represents the status of an autorization request
    /// </summary>
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
        [DataMember(Name="iuid")] internal String Iuid { get; private set; }
        [DataMember(Name="auid")] internal String Auid { get; private set; }
        
        /// <summary>
        /// Indicates if this AuthCode was generated using the Authorize process (using email and password)
        /// </summary>
        public bool IsAuthorized { get { return this.AuthType.IsAuthorized; } }
        /// <summary>
        /// Indicates if this AuthCode was generated using the Identify process (using email and AppSecret)
        /// </summary>
        public bool IsIdentified { get { return this.AuthType.IsIdentified; } }

        /// <summary>
        /// For invalid AuthStatus indicates that the credentials where wrong
        /// </summary>
        public bool IsCredentialError { get { return this.AuthType.StatusCode.Equals(AuthType.STATUS_NOT_AUTHORIZED); } }
        /// <summary>
        /// For invalid AuthStatus indicates that the user has not the required permission
        /// </summary>
        public bool IsPermissionError { get { return this.AuthType.StatusCode.Equals(AuthType.STATUS_NOT_FOUND); } }

        internal static AuthStatus InvalidAuthStatus { get { return new AuthStatus() { AuthType = AuthType.Invalid }; } }
        internal static AuthStatus NotAuthorizedAuthStatus { get { return new AuthStatus() { AuthType = AuthType.NotAuthorized }; } }
        internal static AuthStatus NotFoundAuthStatus { get { return new AuthStatus() { AuthType = AuthType.NotFound }; } }

        /// <summary>
        /// Get a string represenation of this auth-status
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Marshall from a string representation (which you got by SerializeToString())
        /// </summary>
        /// <param name="aSerializedAuthStatus"></param>
        /// <returns></returns>
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
        /// trigger revalidation on the hockeyapp server
        /// </summary>
        /// <returns>true if this status (token) is still valid</returns>
        public async Task<bool> CheckIfStillValidAsync()
        {
            var request = WebRequest.CreateHttp(new Uri(HockeyClient.Current.AsInternal().ApiBaseVersion3 + "apps/" +
                                                            HockeyClient.Current.AsInternal().AppIdentifier + "/identity/validate?" + 
                                                                this.AuthType.IdCode + "=" + this.AuthType.IdFunc(this) , UriKind.Absolute));
            request.Method = "Get";
            request.SetHeader(HttpRequestHeader.UserAgent.ToString(), HockeyClient.Current.AsInternal().UserAgentString);
            return (await DoAuthRequestHandleResponseAsync(request)).IsIdentified;
        }

        internal static async Task<IAuthStatus> DoAuthRequestHandleResponseAsync(HttpWebRequest request)
        {
            WebResponse response = null;
            try
            {
                response = await request.GetResponseAsync();
            }
            catch (WebException webEx)
            {
                if ((webEx.Status == WebExceptionStatus.ConnectFailure) ||
                    (webEx.Status == WebExceptionStatus.SendFailure) ||
                    (webEx.Response == null || String.IsNullOrWhiteSpace(webEx.Response.ContentType)))
                {
                    HockeyClient.Current.AsInternal().HandleInternalUnhandledException(webEx);
                    throw new WebTransferException("Could not connect to server.", webEx);
                }
                else
                {
                    if ((webEx.Response as HttpWebResponse).StatusCode == HttpStatusCode.NotFound)
                    {
                        return AuthStatus.NotFoundAuthStatus;
                    }
                    else if ((webEx.Response as HttpWebResponse).StatusCode == HttpStatusCode.Unauthorized)
                    {
                        return AuthStatus.NotAuthorizedAuthStatus;
                    }
                    //sent if token is invalid
                    else if ((int)(webEx.Response as HttpWebResponse).StatusCode == 422)
                    {
                        return AuthStatus.NotAuthorizedAuthStatus;
                    }
                    else
                    {
                        HockeyClient.Current.AsInternal().HandleInternalUnhandledException(webEx);
                        return AuthStatus.InvalidAuthStatus;
                    }
                }
            }
            IAuthStatus checkedAuthStatus = await TaskEx.Run(() => AuthStatus.FromJson(response.GetResponseStream()));
            return checkedAuthStatus;
        }
    }

}
