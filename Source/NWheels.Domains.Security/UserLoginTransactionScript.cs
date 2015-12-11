using System;
using System.Collections.Generic;
using System.Linq;
using NWheels.Authorization;
using NWheels.Authorization.Core;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.Domains.Security.Core;
using NWheels.Entities.Core;
using NWheels.Exceptions;
using NWheels.Extensions;
using NWheels.Processing;
using NWheels.UI.Core;
using NWheels.UI.Uidl;
using NWheels.Utilities;
using System.Security.Claims;
using NWheels.Entities;

namespace NWheels.Domains.Security
{
    public class UserLoginTransactionScript : ITransactionScript
    {
        private readonly IFramework _framework;
        private readonly IAuthenticationProvider _authenticationProvider;
        private readonly ISessionManager _sessionManager;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UserLoginTransactionScript(IFramework framework, IAuthenticationProvider authenticationProvider, ISessionManager sessionManager)
        {
            _framework = framework;
            _authenticationProvider = authenticationProvider;
            _sessionManager = sessionManager;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Result Execute(string loginName, string password)
        {
            IApplicationDataRepository authenticationContext;
            IQueryable<IUserAccountEntity> userAccountQuery;
            OpenAuthenticationContext(out authenticationContext, out userAccountQuery);

            using ( authenticationContext )
            {
                IUserAccountEntity userAccount;
                UserAccountPrincipal principal;

                var currentSession = _sessionManager.CurrentSession;

                using ( _sessionManager.JoinGlobalSystem() )
                {
                    principal = _authenticationProvider.Authenticate(userAccountQuery, loginName, SecureStringUtility.ClearToSecure(password), out userAccount);
                }

                ExtendUserClaims(principal);
                principal.Identity.DoneExtendingClaims();

                ValidateUidlEndpointLogin(currentSession, principal);

                if ( currentSession.IsGlobalImmutable )
                {
                    _sessionManager.OpenAnonymous(currentSession.Endpoint);
                }

                _sessionManager.As<ICoreSessionManager>().AuthorieSession(principal);

                var result = new Result(principal);
                return result;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void OpenAuthenticationContext(out IApplicationDataRepository context, out IQueryable<IUserAccountEntity> userAccounts)
        {
            var userAccountsContext = _framework.NewUnitOfWork<IUserAccountDataRepository>();

            context = userAccountsContext;
            userAccounts = userAccountsContext.AllUsers;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void ExtendUserClaims(UserAccountPrincipal principal)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void ValidateUidlEndpointLogin(ISession currentSession, UserAccountPrincipal principal)
        {
            var uidlEndpoint = currentSession.Endpoint as IUidlApplicationEndpoint;

            if ( uidlEndpoint != null )
            {
                if ( !uidlEndpoint.UidlApplication.Authorization.TryValidateUser(principal.Identity) )
                {
                    throw new DomainFaultException<LoginFault>(LoginFault.NotAuthorized);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class Result
        {
            internal protected Result(UserAccountPrincipal principal)
            {
                var account = principal.Identity.GetUserAccount();

                FullName = principal.PersonFullName;
                UserType = account.As<IObject>().ContractType.SimpleQualifiedName();
                UserId = account.As<IPersistableObject>().As<IEntityObject>().GetId().Value.ToString();
                UserRoles = principal.GetUserRoles();
                AllClaims = principal.Identity.Claims.Select(c => c.Value).ToArray();
                LastLoginAtUtc = account.LastLoginAtUtc;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string FullName { get; private set; }
            public string UserId { get; private set; }
            public string UserType { get; private set; }
            public string[] UserRoles { get; private set; }
            public string[] AllClaims { get; private set; }
            public DateTime? LastLoginAtUtc { get; private set; }
        }
    }
}
