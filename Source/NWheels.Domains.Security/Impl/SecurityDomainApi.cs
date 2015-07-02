using System.Security.Permissions;
using NWheels.Conventions.Core;
using NWheels.Domains.Security.UI;
using NWheels.UI;
using NWheels.Utilities;

namespace NWheels.Domains.Security.Impl
{
    public class SecurityDomainApi : DomainApiBase, ISecurityDomainApi
    {
        private readonly IFramework _framework;
        private readonly LoginTransactionScript _loginTransaction;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public SecurityDomainApi(IFramework framework, IEntityObjectFactory objectFactory, LoginTransactionScript loginTransaction)
            : base(objectFactory)
        {
            _framework = framework;
            _loginTransaction = loginTransaction;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ILogUserInReply LogUserIn(ILogUserInRequest request)
        {
            _loginTransaction.Execute(request.LoginName, SecureStringUtility.ClearToSecure(request.Password));



            return NewModel<ILogUserInReply>(
                m => m.AuthorizedUidlNodes = new[] { "AAA", "BBB", "CCC" }
            );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Permission.MustAuthenticate(SecurityAction.Demand)]
        public ILogUserOutReply LogUserOut(ILogUserOutRequest request)
        {
            return NewModel<ILogUserOutReply>();
        }
    }
}
