namespace Microsoft.HockeyApp.Services
{
    using System;
    using System.Threading.Tasks;

    internal interface IHttpService
    {
        Task PostAsync(Uri address, byte[] content, string contentType, string contentEncoding, TimeSpan timeout = default(TimeSpan));
    }
}
