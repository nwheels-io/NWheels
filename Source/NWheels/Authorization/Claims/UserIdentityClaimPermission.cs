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
    public class UserIdentityClaimPermission : IPermission
    {
        public void Demand()
        {
            var principal = Thread.CurrentPrincipal;

            if ( principal == null || principal.Identity == null || !principal.Identity.IsAuthenticated )
            {
                throw new AccessDeniedException();
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
