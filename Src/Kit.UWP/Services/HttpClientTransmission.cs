namespace Microsoft.HockeyApp.Services
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    internal class HttpClientTransmission : IHttpService
    {
        internal const string ContentTypeHeader = "Content-Type";
        internal const string ContentEncodingHeader = "Content-Encoding";

        public async Task PostAsync(Uri address, byte[] content, string contentType, string contentEncoding, TimeSpan timeout = default(TimeSpan))
        {
#if DEBUG
            string result = System.Text.Encoding.UTF8.GetString(content, 0, content.Length);
#endif
            HttpClient client = new HttpClient() { Timeout = timeout };
            using (MemoryStream contentStream = new MemoryStream(content))
            {
                var request = new HttpRequestMessage(HttpMethod.Post, address);
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

        public Stream CreateCompressedStream(Stream stream)
        {
            // For some reason for HockeySDK.WinRT compression does not work.
#if WINDOWS_UWP
            return new GZipStream(stream, CompressionMode.Compress);
#else
            return stream;
#endif
        }

        public string GetContentEncoding()
        {
            // For some reason for HockeySDK.WinRT compression does not work.
#if WINDOWS_UWP
            return "gzip";
#else
            return null;
#endif
        }
    }
}
