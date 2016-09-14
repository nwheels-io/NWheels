using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Authorization
{
    public enum AuthorizationFault
    {
        AccessDenied
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum AuthorizationFaultSubCode
    {
        None,
        NotAuthenticated
    }
}
