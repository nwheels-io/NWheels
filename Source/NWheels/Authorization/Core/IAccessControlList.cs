using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace NWheels.Authorization.Core
{
    public interface IAccessControlList
    {
        IEntityAccessControl GetEntityAccessControl(Type entityContractType);
        IReadOnlyCollection<Claim> GetClaims();
    }
}
