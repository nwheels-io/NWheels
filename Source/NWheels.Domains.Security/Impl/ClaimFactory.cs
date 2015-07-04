using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using NWheels.Authorization.Claims;

namespace NWheels.Domains.Security.Impl
{
    public class ClaimFactory
    {
        private readonly IFramework _framework;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ClaimFactory(IFramework framework)
        {
            _framework = framework;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerable<Claim> CreateClaimsFromContainerEntity(IEntityPartClaimsContainer claimsContainer)
        {
            //TODO: use cache of existing permissions/roles/data rules

            var expandedClaims = new List<Claim>();

            using ( var data = _framework.NewUnitOfWork<IUserAccountDataRepository>() )
            {
                foreach ( var includedRole in data.UserRoles.Where(r => claimsContainer.AssociatedRoles.Contains(r)) )
                {
                    expandedClaims.Add(new UserAccountRoleClaim(this, includedRole));
                }

                foreach ( var permission in data.OperationPermissions.Where(p => claimsContainer.AssociatedPermissions.Contains(p)) )
                {
                    expandedClaims.Add(new OperationPermissionClaim(permission.ClaimValue));
                }

                foreach ( var dataRule in data.EntityAccessRules.Where(r => claimsContainer.AssociatedDataRules.Contains(r)) )
                {
                    expandedClaims.Add(new EntityAccessRuleClaim(dataRule.ClaimValue));
                }
            }

            return expandedClaims;
        }
    }
}
