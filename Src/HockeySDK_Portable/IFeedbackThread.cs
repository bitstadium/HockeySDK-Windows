using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.HockeyApp
{
    /// <summary>
    /// interface for a hockeyapp feedback thread
    /// </summary>
    public interface IFeedbackThread
    {
        /// <summary>
        /// time of creation as string
        /// </summary>
        string CreatedAt { get; }
        /// <summary>
        /// email of thread starter
        /// </summary>
        string EMail { get; }
        /// <summary>
        /// unique id
        /// </summary>
        int Id { get; }
        /// <summary>
        /// indicates if this thread was new (not on server yet)
        /// </summary>
        bool IsNewThread { get; }
        /// <summary>
        /// the messages in this thread (newest message last)
        /// </summary>
        List<IFeedbackMessage> Messages { get; }
        /// <summary>
        /// name of the thread
        /// </summary>
        string Name { get; }
        /// <summary>
        /// status
        /// </summary>
        int Status { get; }
        /// <summary>
        /// unique token for this thread
        /// </summary>
        string Token { get; }

        /// <summary>
        /// post a feedback message on this thread
        /// </summary>
        /// <param name="message">message text</param>
        /// <param name="email">[optional] email of sender</param>
        /// <param name="subject">[optional] message subject</param>
        /// <param name="name">[optional] name of sender</param>
        /// <param name="images">[optional] feedback attachments</param>
        /// <returns></returns>
        Task<IFeedbackMessage> PostFeedbackMessageAsync(string message, string email = null, string subject = null, string name = null, IEnumerable<IFeedbackAttachment> images = null);
    }
}
