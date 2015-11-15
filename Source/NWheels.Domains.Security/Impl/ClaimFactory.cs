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
            var expandedClaims = new List<Claim>();

            using ( var context = _framework.NewUnitOfWork<IUserAccountDataRepository>() )
            {
                ListClaimsFromContainer(claimsContainer, expandedClaims);
            }

            return expandedClaims;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ListClaimsFromContainer(IEntityPartClaimsContainer claimsContainer, List<Claim> expandedClaims)
        {
            if ( claimsContainer.AssociatedRoles != null )
            {
                foreach ( var role in claimsContainer.AssociatedRoles )
                {
                    expandedClaims.Add(new UserAccountRoleClaim(this, role));
                    ListClaimsFromContainer(role, expandedClaims);
                }
            }

            if ( claimsContainer.AssociatedPermissions != null )
            {
                foreach ( var permission in claimsContainer.AssociatedPermissions )
                {
                    expandedClaims.Add(new OperationPermissionClaim(permission.ClaimValue));
                }
            }

            if ( claimsContainer.AssociatedEntityAccessRules != null )
            {
                foreach ( var dataRule in claimsContainer.AssociatedEntityAccessRules )
                {
                    expandedClaims.Add(new EntityAccessRuleClaim(dataRule, dataRule.ClaimValue));
                }
            }
        }
    }
}
