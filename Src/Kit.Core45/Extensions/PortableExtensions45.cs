using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.HockeyApp.Extensions
{
    /// <summary>
    /// static extension class for mix extensions on pcl lib
    /// </summary>
    internal static class PortableExtensions45
    {

        /// <summary>
        /// extension method to get web response async
        /// </summary>
        /// <param name="request">the webrequest to send</param>
        /// <returns>the response</returns>
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

        /// <summary>
        /// get the request stream asynchronously
        /// </summary>
        /// <param name="request">the request</param>
        /// <returns>the request stream to write on</returns>
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

        /// <summary>
        /// extension method for type to get a runtime property
        /// </summary>
        /// <param name="self">type to get the property from</param>
        /// <param name="propertyName">name of the property to retrieve</param>
        /// <returns>PropertyInfo from type</returns>
        public static PropertyInfo GetProperty(this Type self, string propertyName)
        {
            return self.GetRuntimeProperty(propertyName);
        }

    }
}
