using System;
using System.IO;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Microsoft.HockeyApp.Services
{
    sealed class WinFormsHttpService : IHttpService
    {
        internal const string ContentTypeHeader = "Content-Type";
        internal const string ContentEncodingHeader = "Content-Encoding";

        public Stream CreateCompressedStream(Stream stream)
        {
            return new GZipStream(stream, CompressionMode.Compress);
        }

        public string GetContentEncoding()
        {
            return "gzip";
        }

        public async Task PostAsync(Uri address, byte[] content, string contentType, string contentEncoding, TimeSpan timeout = default(TimeSpan))
        {
            var client = new HttpClient() { Timeout = timeout };
            using (var contentStream = new MemoryStream(content))
            using (var request = new HttpRequestMessage(HttpMethod.Post, address))
            {
                request.Content = new StreamContent(contentStream);
                if (!string.IsNullOrEmpty(contentType))
                {
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                }

                if (!string.IsNullOrEmpty(contentEncoding))
                {
                    request.Content.Headers.Add(ContentEncodingHeader, contentEncoding);
                }

                await client.SendAsync(request);
            }
        }
    }
}
