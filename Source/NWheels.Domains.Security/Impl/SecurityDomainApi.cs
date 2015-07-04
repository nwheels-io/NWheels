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
            var identityInfo = _loginTransaction.Execute(request.LoginName, SecureStringUtility.ClearToSecure(request.Password));
            var reply = NewModel<ILogUserInReply>();

            reply.AuthorizedUidlNodes = new string[0];
            reply.FullName = identityInfo.PersonFullName;
            reply.Roles = identityInfo.GetUserRoles();

            return reply;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ILogUserOutReply LogUserOut(ILogUserOutRequest request)
        {
            return NewModel<ILogUserOutReply>();
        }
    }
}
