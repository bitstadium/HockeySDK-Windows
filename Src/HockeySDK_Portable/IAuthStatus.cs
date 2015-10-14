using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.HockeyApp
{
    /// <summary>
    /// interface for an authorization state
    /// </summary>
    public interface IAuthStatus
    {
        /// <summary>
        /// trigger revalidation on the hockeyapp server
        /// </summary>
        /// <returns>true if this status (token) is still valid</returns>
        Task<bool> CheckIfStillValidAsync();
        /// <summary>
        /// Indicates if this AuthCode was generated using the Authorize process (using email and password)
        /// </summary>
        bool IsAuthorized { get; }
        /// <summary>
        /// Indicates if this AuthCode was generated using the Identify process (using email and AppSecret)
        /// </summary>
        bool IsIdentified { get; }
        /// <summary>
        /// For invalid AuthStatus indicates that the user has not the required permission
        /// </summary>
        bool IsPermissionError { get; }
        /// <summary>
        /// For invalid AuthStatus indicates that the credentials where wrong
        /// </summary>
        bool IsCredentialError { get; }

    }
}
