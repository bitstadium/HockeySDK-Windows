using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HockeyApp
{
    public interface IAuthStatus
    {
        Task<bool> CheckIfStillValid();
        bool IsAuthorized { get; }
        bool IsIdentified { get; }
    
    }
}
