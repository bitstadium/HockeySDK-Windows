using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HockeyApp.Extensions
{
    public static class PortableExtensions45
    {

        public static Task<HttpWebResponse> GetResponseAsync(this WebRequest request)
        {
            var taskComplete = new TaskCompletionSource<HttpWebResponse>();
            request.BeginGetResponse(asyncResponse =>
            {
                try
                {
                    HttpWebRequest responseRequest = (HttpWebRequest)asyncResponse.AsyncState;
                    HttpWebResponse someResponse =
                       (HttpWebResponse)responseRequest.EndGetResponse(asyncResponse);
                    taskComplete.TrySetResult(someResponse);
                }
                catch (WebException webExc)
                {
                    HttpWebResponse failedResponse = (HttpWebResponse)webExc.Response;
                    //taskComplete.TrySetResult(failedResponse);
                    taskComplete.SetException(webExc);
                }
            }, request);
            return taskComplete.Task;
        }


        public static Task<Stream> GetRequestStreamAsync(this WebRequest request)
        {
            var taskComplete = new TaskCompletionSource<Stream>();
            request.BeginGetRequestStream(asyncStream =>
            {
                try
                {
                    Stream requestStream = (Stream)request.EndGetRequestStream(asyncStream);
                    taskComplete.TrySetResult(requestStream);
                }
                catch (Exception e)
                {
                    taskComplete.TrySetException(e);
                }
            }, request);
            return taskComplete.Task;
        }

        public static PropertyInfo GetProperty(this Type self, string propertyName)
        {
            return self.GetRuntimeProperty(propertyName);
        }

    }
}
