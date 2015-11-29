using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Domains.Security
{
    public enum LoginFault
    {
        LoginIncorrect,
        PasswordExpired,
        AccountLockedOut,
        NotAuthorized
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum LogoutFault
    {
        NotLoggedIn
    }
}
