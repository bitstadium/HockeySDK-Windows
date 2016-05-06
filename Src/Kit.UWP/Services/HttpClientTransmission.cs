namespace Microsoft.HockeyApp.Services
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    internal class HttpClientTransmission : IHttpService
    {
        internal const string ContentTypeHeader = "Content-Type";
        internal const string ContentEncodingHeader = "Content-Encoding";

        public async Task PostAsync(Uri address, byte[] content, string contentType, string contentEncoding, TimeSpan timeout = default(TimeSpan))
        {
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
    }
}
