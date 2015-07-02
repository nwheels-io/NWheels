using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Authorization
{
    [EnumClaimContract(EnumClaimKind.OperationPermission)]
    public enum EntityPermission
    {
        Create,
        Retrieve,
        Update,
        Delete,
    }
}
