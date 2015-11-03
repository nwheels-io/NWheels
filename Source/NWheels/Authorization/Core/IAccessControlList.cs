using System.Collections.Generic;
using System.Security.Claims;

namespace NWheels.Authorization.Core
{
    public interface IAccessControlList
    {
        IEntityAccessControl<TEntity> GetEntityAccessControl<TEntity>();
        IReadOnlyCollection<Claim> GetClaims();
    }
}
