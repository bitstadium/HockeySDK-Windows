namespace Microsoft.HockeyApp.Services
{
    using System;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using Extensions;

    internal class HttpService : IHttpService
    {
        internal const string ContentTypeHeader = "Content-Type";
        internal const string ContentEncodingHeader = "Content-Encoding";
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(100);

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpService"/> class.
        /// </summary>
        internal HttpService()
        {
        }

        public async Task PostAsync(Uri address, byte[] content, string contentType, string contentEncoding, TimeSpan timeout = default(TimeSpan))
        {
            if (address == null) throw new ArgumentNullException("address");
            if (content == null) throw new ArgumentNullException("content");
            if (contentType == null) throw new ArgumentNullException("contentType");

            var request = (HttpWebRequest)WebRequest.Create(address);
            request.Method = "POST";

            if (!string.IsNullOrEmpty(contentEncoding))
            {
                request.Headers[ContentEncodingHeader] = contentEncoding;
            }

            using (Stream requestStream = await request.GetRequestStreamAsync())
            {
                await requestStream.WriteAsync(content, 0, content.Length);
            }

            using (WebResponse response = await request.GetResponseAsync()) {}
        }
    }
}


