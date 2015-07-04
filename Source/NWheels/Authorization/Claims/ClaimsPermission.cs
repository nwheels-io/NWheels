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
    public class ClaimsPermission : IPermission
    {
        private readonly string _requiredClaimType;
        private readonly string[] _requiredClaimValues;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ClaimsPermission(string requiredClaimType, params string[] requiredClaimValues)
        {
            _requiredClaimType = requiredClaimType;
            _requiredClaimValues = requiredClaimValues;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Demand()
        {
            var principal = Thread.CurrentPrincipal as ClaimsPrincipal;

            if ( principal == null && _requiredClaimValues.Length > 0 )
            {
                throw new AccessDeniedException();
            }

            foreach ( var value in _requiredClaimValues )
            {
                var requiredClaimValue = value;

                if ( !principal.HasClaim(_requiredClaimType, requiredClaimValue) )
                {
                    throw new AccessDeniedException(_requiredClaimType, requiredClaimValue);
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
    }
}
