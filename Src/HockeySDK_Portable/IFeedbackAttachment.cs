using System;
using System.Threading.Tasks;

namespace Microsoft.HockeyApp
{
    /// <summary>
    /// Interface for feedback attachments (documents, images)
    /// </summary>
    public interface IFeedbackAttachment
    {
        /// <summary>
        /// unique Id of the attachment
        /// </summary>
        string Id { get; }
        /// <summary>
        /// Timestamp of creation
        /// </summary>
        string CreatedAt { get; }
        /// <summary>
        /// Remote URL where this attachment is available
        /// </summary>
        string RemoteURL { get; }
        /// <summary>
        /// Name of the file when it was uploaded
        /// </summary>
        string FileName { get; set; }
        /// <summary>
        /// Bytes (usually only used when uploading attachments)
        /// </summary>
        byte[] DataBytes { get; set; }
        /// <summary>
        /// Mime content type
        /// </summary>
        string ContentType { get; set; }
        /// <summary>
        /// Load attachment to local storage
        /// </summary>
        /// <returns>true if successfull</returns>
        Task<bool> LoadAttachmentFromServer();
    }
}
