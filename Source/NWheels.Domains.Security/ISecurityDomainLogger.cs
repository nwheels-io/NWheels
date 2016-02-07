using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Logging;

namespace NWheels.Domains.Security
{
    public interface ISecurityDomainLogger : IApplicationEventLogger
    {
        [LogInfo(ToAuditLog = true)]
        void UserLoggedIn(
            [Detail(Indexed = true, IncludeInSingleLineText = true)] string loginName,
            [Detail(Indexed = true)] string userId,
            [Detail(Indexed = true)] string userEmail, 
            string sessionId);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [LogInfo(ToAuditLog = true)]
        void UserLoggedOut(
            [Detail(Indexed = true, IncludeInSingleLineText = true)] string loginName,
            [Detail(Indexed = true)] string userId,
            [Detail(Indexed = true)] string userEmail, 
            string sessionId);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [LogError(ToAuditLog = true)]
        void UserLoginFailed(
            [Detail(Indexed = true, IncludeInSingleLineText = true)] string loginName,
            [Detail(Indexed = true, IncludeInSingleLineText = true)] LoginFault reason);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        [LogWarning(ToAuditLog = true)]
        void AccountLockedOut(
            [Detail(Indexed = true, IncludeInSingleLineText = true)] string loginName,
            [Detail(Indexed = true)] string userId,
            [Detail(Indexed = true)] string userEmail);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [LogWarning]
        void UserNotFound(string loginName);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [LogVerbose]
        void UserAuthenticated(string loginName);
    }
}
