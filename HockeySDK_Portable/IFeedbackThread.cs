using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HockeyApp
{
    public interface IFeedbackThread
    {
        string CreatedAt { get; }
        string EMail { get; }
        int Id { get; }
        bool IsNewThread { get; }
        List<IFeedbackMessage> Messages { get; }
        string Name { get; }
        int Status { get; }
        string Token { get; }

        Task<IFeedbackMessage> PostFeedbackMessageAsync(string message, string email = null, string subject = null, string name = null, IEnumerable<IFeedbackAttachment> images = null);
    }
}
