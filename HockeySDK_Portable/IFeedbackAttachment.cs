using System;
using System.Threading.Tasks;

namespace HockeyApp
{
    public interface IFeedbackAttachment
    {
        string Id { get; }
        string CreatedAt { get; }
        string RemoteURL { get; }
        string FileName { get; set; }
        byte[] DataBytes { get; set; }
        string ContentType { get; set; }
        Task<bool> LoadAttachmentFromServer();
    }
}
