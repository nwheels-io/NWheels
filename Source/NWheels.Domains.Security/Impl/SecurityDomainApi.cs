using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using NWheels.Conventions.Core;
using NWheels.Domains.Security.UI;
using NWheels.UI;

namespace NWheels.Domains.Security.Impl
{
    internal class SecurityDomainApi : DomainApiBase, ISecurityDomainApi
    {
        public SecurityDomainApi(IEntityObjectFactory objectFactory)
            : base(objectFactory)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ILogUserInReply LogUserIn(ILogUserInRequest request)
        {
            return NewModel<ILogUserInReply>(
                m => m.UserId = null,
                m => m.AuthorizedUidlNodes = new[] { "AAA", "BBB", "CCC" });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ILogUserOutReply LogUserOut(ILogUserOutRequest request)
        {
            return NewModel<ILogUserOutReply>();
        }
    }
}
