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
        //private readonly Dictionary<string, ClaimFactoryMethodCallback> _factoryMethodByClaimType;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ClaimFactory(IFramework framework)
        {
            _framework = framework;
            //_factoryMethodByClaimType = InitializeFactoryMethods();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerable<Claim> CreateClaimsFromContainerEntity(IEntityPartClaimsContainer claimsContainer)
        {
            var expandedClaims = new List<Claim>();

            using ( var data = _framework.NewUnitOfWork<IUserAccountDataRepository>() )
            {
                foreach ( var includedRole in data.UserRoles.Where(r => claimsContainer.AssociatedRoles.Contains(r)) )
                {
                    expandedClaims.Add(new UserAccountRoleClaim(this, includedRole));
                }

                foreach ( var permission in data.OperationPermissions.Where(p => claimsContainer.AssociatedPermissions.Contains(p)) )
                {
                    expandedClaims.Add(new OperationPermissionClaim(ParseClaimEnumValue(permission.ClaimValueType, permission.ClaimValue)));
                }

                foreach ( var dataRule in data.EntityAccessRules.Where(r => claimsContainer.AssociatedDataRules.Contains(r)) )
                {
                    expandedClaims.Add(new EntityAccessRuleClaim(ParseClaimEnumValue(dataRule.ClaimValueType, dataRule.ClaimValue)));
                }
            }

            return expandedClaims;
        }

        ////-----------------------------------------------------------------------------------------------------------------------------------------------------

        //public Claim CreateClaimFromString(string claimString)
        //{
        //    var claimStringParts = claimString.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

        //    if ( claimStringParts.Length == 3 )
        //    {
        //        var claimType = claimStringParts[0];
        //        var claimValue = claimStringParts[1];
        //        var claimValueType = claimStringParts[2];

        //        ClaimFactoryMethodCallback factoryMethod;

        //        if ( _factoryMethodByClaimType.TryGetValue(claimType, out factoryMethod) )
        //        {
        //            return factoryMethod(claimValueType, claimValue);
        //        }
        //    }

        //    throw new FormatException("Claim string is of incorrect format: " + claimString);
        //}

        ////-----------------------------------------------------------------------------------------------------------------------------------------------------

        //private Dictionary<string, ClaimFactoryMethodCallback> InitializeFactoryMethods()
        //{
        //    return new Dictionary<string, ClaimFactoryMethodCallback> {
        //        {
        //            UserRoleClaim.UserRoleClaimTypeString, 
        //            (valueType, value) => new UserAccountRoleClaim(this, valueType, value)
        //        }, 
        //        {
        //            OperationPermissionClaim.OperationPermissionClaimTypeString, 
        //            (valueType, value) => new OperationPermissionClaim(ParseClaimEnumValue(valueType, value))
        //        }, 
        //        {
        //            EntityAccessRuleClaim.EntityAccessRuleClaimTypeString, 
        //            (valueType, value) => new EntityAccessRuleClaim(ParseClaimEnumValue(valueType, value))
        //        },
        //    };
        //}

        ////-----------------------------------------------------------------------------------------------------------------------------------------------------

        //public static string ToClaimString(Claim claim)
        //{
        //    return string.Format("{0}:{1}:{2}", claim.Type, claim.Value, claim.ValueType);
        //}

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static object ParseClaimEnumValue(string claimValueType, string claimValue)
        {
            var claimValueParts = claimValue.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            
            if ( claimValueParts.Length == 2 )
            {
                var enumType = Type.GetType(claimValueType, throwOnError: true);
                return Enum.Parse(enumType, claimValueParts[1]);
            }

            throw new FormatException("Claim string is of incorrect format");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private delegate Claim ClaimFactoryMethodCallback(string claimValueType, string claimValue);
    }
}
