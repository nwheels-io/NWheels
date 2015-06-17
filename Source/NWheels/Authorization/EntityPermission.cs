using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Authorization
{
    [AuthorizationContract.Claims]
    public enum EntityPermission
    {
        Create,
        Retrieve,
        Update,
        Delete
    }
}
