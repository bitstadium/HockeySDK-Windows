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
    public static class WebRequestExtension
    {
        /// <summary>
        /// Set a http header on a Web-Request. Either by setting the property or by adding to the Headers dict.
        /// </summary>
        /// <param name="request">self</param>
        /// <param name="header">header key</param>
        /// <param name="value">header value</param>
        public static void SetHeader(this WebRequest request, string header, string value)
        {
            // Retrieve the property through reflection.
            PropertyInfo PropertyInfo = request.GetType().GetProperty(header.Replace("-", string.Empty));
            // Check if the property is available.
            if (PropertyInfo != null)
            {
                // Set the value of the header.
                PropertyInfo.SetValue(request, value, null);
            }
            else
            {
                // Set the value of the header.
                request.Headers[header] = value;
            }
        }

        /// <summary>
        /// Set form encoded postData on a webrequest.
        /// </summary>
        /// <param name="request">self</param>
        /// <param name="postData">string with form encoded data</param>
        /// <returns></returns>
        public static async Task SetPostDataAsync(this WebRequest request, string postData)
        {
            byte[] dataStream = Encoding.UTF8.GetBytes(postData);
            request.ContentType = "application/x-www-form-urlencoded";
            request.SetHeader(HttpRequestHeader.ContentEncoding.ToString(), Encoding.UTF8.WebName.ToString());
            Stream stream = await request.GetRequestStreamAsync();
            stream.Write(dataStream, 0, dataStream.Length);
            stream.Dispose();
        }

    }
}
