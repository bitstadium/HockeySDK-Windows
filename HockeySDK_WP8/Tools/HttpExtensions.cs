using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
        using System.Net;
using System.Threading.Tasks;
using System.IO;

namespace HockeyApp.Tools
{
    public static class HttpExtensions
    {

        public static Task<WebResponse> GetResponseTaskAsync(this HttpWebRequest webRequest)
        {
            Func<Task<WebResponse>> function = (() => Task.Factory.FromAsync<WebResponse>(webRequest.BeginGetResponse, webRequest.EndGetResponse,TaskCreationOptions.None));
#if WP8
            return Task.Run(function);
#else
            return TaskEx.Run(function);
#endif
        }

        public static async Task<bool> SetPostDataAsync(this HttpWebRequest request, string postData)
        {
            byte[] dataStream = Encoding.UTF8.GetBytes(postData);
            request.ContentType = Constants.ContentTypeUrlEncoded;
#if WP8
            request.ContentLength = dataStream.Length;
#endif
            request.Headers[HttpRequestHeader.ContentEncoding] = Encoding.UTF8.WebName;
            Stream stream = await request.GetRequestStreamAsync();
            stream.Write(dataStream, 0, dataStream.Length);
            stream.Close();
            return true;
        }


        public static Task<Stream> GetRequestStreamAsync(this HttpWebRequest request)
        {
            var taskComplete = new TaskCompletionSource<Stream>();
            request.BeginGetRequestStream(asyncResponse =>
            {
                try
                {
                    HttpWebRequest responseRequest = (HttpWebRequest)asyncResponse.AsyncState;
                    var someResponse = responseRequest.EndGetRequestStream(asyncResponse);
                    taskComplete.TrySetResult(someResponse);
                }
                catch (WebException webExc)
                {
                    taskComplete.TrySetException(webExc);
                }
            }, request);
            return taskComplete.Task;
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
