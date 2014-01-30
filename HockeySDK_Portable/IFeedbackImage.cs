using System;

namespace HockeyApp
{
    public interface IFeedbackImage
    {
        string FileName { get; set; }
        byte[] ImageBytes { get; set; }
        string RemoteURL { get; }
        string ContentType { get; }
    }
}
