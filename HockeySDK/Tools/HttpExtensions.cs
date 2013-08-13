using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
        using System.Net;
using System.Threading.Tasks;

namespace HockeyApp.Tools
{
    public static class HttpExtensions
    {

        public static Task<WebResponse> GetResponseTaskAsync(this HttpWebRequest webRequest)
        {
            Func<Task<WebResponse>> function = (() => Task.Factory.FromAsync<WebResponse>(webRequest.BeginGetResponse, webRequest.EndGetResponse,TaskCreationOptions.None));
            return Task.Run(function);
        }
    }

    public static class HttpMethod
    {
        public static string Head { get { return "HEAD"; } }
        public static string Post { get { return "POST"; } }
        public static string Put { get { return "PUT"; } }
        public static string Get { get { return "GET"; } }
        public static string Delete { get { return "DELETE"; } }
        public static string Trace { get { return "TRACE"; } }
        public static string Options { get { return "OPTIONS"; } }
        public static string Connect { get { return "CONNECT"; } }
        public static string Patch { get { return "PATCH"; } }
    }
}
