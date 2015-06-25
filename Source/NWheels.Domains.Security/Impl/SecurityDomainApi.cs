using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Domains.Security.UI;

namespace NWheels.Domains.Security.Impl
{
    internal class SecurityDomainApi : ISecurityDomainApi
    {
        public ILogUserInReply LogUserIn(ILogUserInRequest request)
        {
            throw new NotImplementedException();
        }

        public ILogUserInReply LogUserOut()
        {
            throw new NotImplementedException();
        }
    }
}
