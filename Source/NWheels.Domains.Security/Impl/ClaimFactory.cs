using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using NWheels.Authorization.Claims;
using NWheels.Hosting;

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
                var allRoles = data.UserRoles.ToDictionary(r => r.ClaimValue);
                var allPermissions = data.OperationPermissions.ToDictionary(p => p.ClaimValue);
                var allDataRules = data.EntityAccessRules.ToDictionary(r => r.ClaimValue);

                if ( claimsContainer.AssociatedRoles != null )
                {
                    foreach ( var includedRole in claimsContainer.AssociatedRoles.Select(s => allRoles[s]) )
                    {
                        expandedClaims.Add(new UserAccountRoleClaim(this, includedRole));
                    }
                }

                if ( claimsContainer.AssociatedPermissions != null )
                {
                    foreach ( var permission in claimsContainer.AssociatedPermissions.Select(s => allPermissions[s]) )
                    {
                        expandedClaims.Add(new OperationPermissionClaim(permission.ClaimValue));
                    }
                }

                if (claimsContainer.AssociatedDataRules != null)
                {
                    foreach ( var dataRule in claimsContainer.AssociatedDataRules.Select(s => allDataRules[s]) )
                    {
                        expandedClaims.Add(new EntityAccessRuleClaim(dataRule.ClaimValue));
                    }
                }
            }

            return expandedClaims;
        }
    }
}
