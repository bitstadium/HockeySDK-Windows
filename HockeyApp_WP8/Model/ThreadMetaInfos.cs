using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HockeyApp.Model
{
    public class FeedbackThreadMetaInfos
    {
        public FeedbackThreadMetaInfos(string subject, string username, string email, string id)
        {
            this.Subject = subject;
            this.Username = username;
            this.Email = email;
            this.Id = id;
        }

        public string Subject { get; protected set; }
        public string Username { get; protected set; }
        public string Email { get; protected set; }
        public string Id { get; protected set; }
    }
}
