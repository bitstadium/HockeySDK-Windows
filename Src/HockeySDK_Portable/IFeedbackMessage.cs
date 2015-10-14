using System;
using System.Collections.Generic;
namespace Microsoft.HockeyApp
{
    /// <summary>
    /// Represents a message in a feedback thread
    /// </summary>
    public interface IFeedbackMessage
    {
        /// <summary>
        /// App id of the app this feedback message belongs to
        /// </summary>
        string AppId { get; }
        /// <summary>
        /// App version where the feedback relates to
        /// </summary>
        string AppVersionId { get; }
        /// <summary>
        /// Raw text of the message
        /// </summary>
        string CleanText { get; }
        /// <summary>
        /// Timestamp of creation
        /// </summary>
        DateTime Created { get; }
        /// <summary>
        /// Timestamp of creation as string
        /// </summary>
        string CreatedAt { get; }
        /// <summary>
        /// email of the message sender
        /// </summary>
        string Email { get; set; }
        /// <summary>
        /// Gravatar hash of the senders email
        /// </summary>
        string GravatarHash { get; set; }
        /// <summary>
        /// unique id of the message
        /// </summary>
        int Id { get; }
        /// <summary>
        /// Message is only internal
        /// </summary>
        bool? Internal { get; }
        /// <summary>
        /// Device model
        /// </summary>
        string Model { get; }
        /// <summary>
        /// Name of sender
        /// </summary>
        string Name { get; set; }
        /// <summary>
        ///  Device oem
        /// </summary>
        string Oem { get; }
        /// <summary>
        /// corresponding OS Version
        /// </summary>
        string OSVersion { get; }
        /// <summary>
        /// subject 
        /// </summary>
        string Subject { get; set; }
        /// <summary>
        /// text (can include html)
        /// </summary>
        string Text { get; set; }
        /// <summary>
        /// Token for message
        /// </summary>
        string Token { get; }
        /// <summary>
        /// User
        /// </summary>
        string UserString { get; }
        /// <summary>
        /// via-flag (indicates if from web/api ..)
        /// </summary>
        int Via { get; }
        /// <summary>
        /// via-flag as string
        /// </summary>
        string ViaAsString { get; }
        /// <summary>
        /// message attachments
        /// </summary>
        IEnumerable<IFeedbackAttachment> Attachments { get; }
    }
}
