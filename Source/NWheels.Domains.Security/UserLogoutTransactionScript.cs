using System;
using System.Linq;
using NWheels.Authorization;
using NWheels.Authorization.Core;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.Domains.Security.Core;
using NWheels.Extensions;
using NWheels.Processing;
using NWheels.Utilities;

namespace NWheels.Domains.Security
{
    [TransactionScript(AuditName = "SecurityDomain.Logout")]
    public class UserLogoutTransactionScript : ITransactionScript
    {
        private readonly ICoreSessionManager _sessionManager;
        private readonly ISecurityDomainLogger _logger;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UserLogoutTransactionScript(ICoreSessionManager sessionManager, ISecurityDomainLogger logger)
        {
            _sessionManager = sessionManager;
            _logger = logger;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void Execute()
        {
            var session = Session.Current;
            
            _sessionManager.CloseCurrentSession();
            _sessionManager.As<ISessionManager>().OpenAnonymous(session.Endpoint);
            _sessionManager.As<ICoreSessionManager>().SetSessionUserInfo(Session.Current.Id, newCulture: session.Culture);

            _logger.UserLoggedOut(session.UserIdentity.LoginName, session.UserIdentity.UserId, session.UserIdentity.EmailAddress, session.Id);
        }
    }
}
