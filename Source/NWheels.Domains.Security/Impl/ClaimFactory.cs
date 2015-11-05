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
        private Dictionary<string, IUserRoleEntity> _allRoles;
        private Dictionary<string, IOperationPermissionEntity> _allPermissions;
        private Dictionary<string, IEntityAccessRuleEntity> _allDataRules;

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
                RefreshCache(context); //TODO: use real caching of all defined claims
                ListClaimsFromContainer(claimsContainer, expandedClaims);
            }

            return expandedClaims;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void RefreshCache(IUserAccountDataRepository context)
        {
            _allRoles = context.UserRoles.ToDictionary(r => r.ClaimValue);
            _allPermissions = context.OperationPermissions.ToDictionary(p => p.ClaimValue);
            _allDataRules = context.EntityAccessRules.ToDictionary(r => r.ClaimValue);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ListClaimsFromContainer(IEntityPartClaimsContainer claimsContainer, List<Claim> expandedClaims)
        {
            if ( claimsContainer.AssociatedRoles != null )
            {
                foreach ( var role in claimsContainer.AssociatedRoles.Select(s => _allRoles[s]) )
                {
                    expandedClaims.Add(new UserAccountRoleClaim(this, role));
                    ListClaimsFromContainer(role, expandedClaims);
                }
            }

            if ( claimsContainer.AssociatedPermissions != null )
            {
                foreach ( var permission in claimsContainer.AssociatedPermissions.Select(s => _allPermissions[s]) )
                {
                    expandedClaims.Add(new OperationPermissionClaim(permission.ClaimValue));
                }
            }

            if ( claimsContainer.AssociatedDataRules != null )
            {
                foreach ( var dataRule in claimsContainer.AssociatedDataRules.Select(s => _allDataRules[s]) )
                {
                    expandedClaims.Add(new EntityAccessRuleClaim(dataRule, dataRule.ClaimValue));
                }
            }
        }
    }
}
