using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NWheels.Exceptions;

namespace NWheels.Authorization.Claims
{
    public class EnumClaimsPermission : IPermission
    {
        private readonly object[] _requiredClaimEnumValues;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EnumClaimsPermission(object[] requiredClaimEnumValues)
        {
            _requiredClaimEnumValues = requiredClaimEnumValues;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Demand()
        {
            var principal = Thread.CurrentPrincipal as ClaimsPrincipal;

            if ( principal == null && _requiredClaimEnumValues.Length > 0 )
            {
                throw new AccessDeniedException();
            }

            foreach ( var value in _requiredClaimEnumValues )
            {
                var requiredValue = value;

                if ( !principal.HasClaim(presentClaim => MatchClaim(presentClaim, requiredValue)) )
                {
                    throw new AccessDeniedException(requiredValue);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IPermission Copy()
        {
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IPermission Intersect(IPermission target)
        {
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsSubsetOf(IPermission target)
        {
            return false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IPermission Union(IPermission target)
        {
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void FromXml(SecurityElement e)
        {
            throw new NotSupportedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public SecurityElement ToXml()
        {
            throw new NotSupportedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool MatchClaim(Claim claim, object requiredEnumValue)
        {
            var enumClaim = claim as EnumClaimBase;

            if ( enumClaim != null )
            {
                return enumClaim.MatchEnumValue(requiredEnumValue);
            }
            else
            {
                return false;
            }
        }
    }
}
