using System;
using System.Security.Permissions;
using NWheels.Authorization.Impl;
using NWheels.Conventions.Core;
using NWheels.Domains.Security.Core;
using NWheels.Domains.Security.UI;
using NWheels.UI;
using NWheels.UI.Factories;
using NWheels.Utilities;

namespace NWheels.Domains.Security.Impl
{
    public class SecurityDomainApi : DomainApiBase, ISecurityDomainApi
    {
        private readonly IFramework _framework;
        private readonly UserLoginTransactionScript _loginTransaction;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public SecurityDomainApi(IFramework framework, ViewModelObjectFactory objectFactory, UserLoginTransactionScript loginTransaction)
            : base(objectFactory)
        {
            _framework = framework;
            _loginTransaction = loginTransaction;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ILogUserInReply LogUserIn(ILogUserInRequest request)
        {
            throw new NotSupportedException();

            //var session = _loginTransaction.Execute(request.LoginName, request.Password);
            //var reply = NewModel<ILogUserInReply>();

            //reply.AuthorizedUidlNodes = new string[0];
            //reply.FullName = session.UserIdentity.PersonFullName;
            //reply.Roles = session.UserIdentity.GetUserRoles();

            //return reply;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ILogUserOutReply LogUserOut(ILogUserOutRequest request)
        {
            return NewModel<ILogUserOutReply>();
        }
    }
}
