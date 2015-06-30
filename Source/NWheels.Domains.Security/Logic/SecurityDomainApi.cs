using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using NWheels.Conventions.Core;
using NWheels.Domains.Security.UI;
using NWheels.UI;

namespace NWheels.Domains.Security.Logic
{
    public class SecurityDomainApi : DomainApiBase, ISecurityDomainApi
    {
        private readonly IFramework _framework;
        private readonly LoginTransactionScript _loginScript;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public SecurityDomainApi(IFramework framework, IEntityObjectFactory objectFactory, LoginTransactionScript loginScript)
            : base(objectFactory)
        {
            _framework = framework;
            _loginScript = loginScript;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ILogUserInReply LogUserIn(ILogUserInRequest request)
        {
            var user = _loginScript.Execute(request.LoginName, request.Password);

            return NewModel<ILogUserInReply>(
                m => m.UserFullName = user.FullName,
                m => m.AuthorizedUidlNodes = new[] { "AAA", "BBB", "CCC" });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ILogUserOutReply LogUserOut(ILogUserOutRequest request)
        {
            return NewModel<ILogUserOutReply>();
        }
    }
}
