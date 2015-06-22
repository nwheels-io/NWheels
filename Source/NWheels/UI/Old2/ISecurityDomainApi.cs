using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    public interface ISecurityDomainApi
    {
        [DomainApiFault(typeof(LoginFault))]
        void LogUserIn(string loginName, string password);

        [DomainApiFault(typeof(LogoutFault))]
        void LogUserOut();

        bool IsOperationAuthorized(string operationName, string resourceType, string resourceId);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------
    
    public enum LoginFault
    {
        LoginIncorrect,
        PasswordExpired,
        AccountLockedOut
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum LogoutFault
    {
        NotLoggedIn
    }
}
