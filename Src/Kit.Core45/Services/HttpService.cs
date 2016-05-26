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
//#if DEBUG
//            string result = System.Text.Encoding.UTF8.GetString(content, 0, content.Length);
//#endif
            var request = WebRequest.CreateHttp(address);
            request.Method = "POST";
            request.ContentType = contentType;

            if (!string.IsNullOrEmpty(contentEncoding))
            {
                request.Headers[ContentEncodingHeader] = contentEncoding;
            }

            using (Stream stream = await request.GetRequestStreamAsync())
            {
                stream.Write(content, 0, content.Length);
                stream.Flush();
            }

            using (WebResponse response = await request.GetResponseAsync()) { }
        }

        public Stream CreateCompressedStream(Stream stream)
        {
            return stream;
        }

        public string GetContentEncoding()
        {
            return null;
        }
    }
}


