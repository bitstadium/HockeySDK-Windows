using System;
using System.Collections.Generic;
namespace HockeyApp
{
    public interface IFeedbackMessage
    {
        string AppId { get; }
        string AppVersionId { get; }
        string CleanText { get; }
        DateTime Created { get; }
        string CreatedAt { get; }
        string Email { get; set; }
        string GravatarHash { get; set; }
        int Id { get; }
        bool? Internal { get; }
        string Model { get; }
        string Name { get; set; }
        string Oem { get; }
        string OSVersion { get; }
        string Subject { get; set; }
        string Text { get; set; }
        string Token { get; }
        string UserString { get; }
        int Via { get; }
        string ViaAsString { get; }
        IEnumerable<IFeedbackAttachment> Attachments { get; }
    }
}
