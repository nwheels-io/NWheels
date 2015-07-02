using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NWheels.Exceptions;

namespace NWheels.Authorization.Claims
{
    public class AnonymousAccessPermission : IPermission
    {
        public void Demand()
        {
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
            return true;
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
