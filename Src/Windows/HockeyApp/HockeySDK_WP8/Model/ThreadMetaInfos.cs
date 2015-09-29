using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HockeyApp.Model
{
    /// <summary>
    /// meta information for current feedback thread
    /// </summary>
    public class FeedbackThreadMetaInfos
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FeedbackThreadMetaInfos"/> class.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="username">The username.</param>
        /// <param name="email">The email.</param>
        /// <param name="id">The identifier.</param>
        public FeedbackThreadMetaInfos(string subject, string username, string email, string id)
        {
            this.Subject = subject;
            this.Username = username;
            this.Email = email;
            this.Id = id;
        }

        /// <summary>
        /// subject of the thread
        /// </summary>
        public string Subject { get; protected set; }
        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; protected set; }
        /// <summary>
        /// last used email.
        /// </summary>
        public string Email { get; protected set; }
        /// <summary>
        /// thread id
        /// </summary>
        public string Id { get; protected set; }
    }
}
