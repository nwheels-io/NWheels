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
using NWheels.Endpoints.Core;
using NWheels.Entities;

namespace NWheels.Domains.Security
{
    [SecurityCheck.AllowAnonymous]
    public class UserLoginTransactionScript : ITransactionScript
    {
        private readonly IFramework _framework;
        private readonly ITypeMetadataCache _metadataCache;
        private readonly IAuthenticationProvider _authenticationProvider;
        private readonly ISessionManager _sessionManager;
        private readonly ISecurityDomainLogger _logger;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UserLoginTransactionScript(
            IFramework framework, 
            ITypeMetadataCache metadataCache, 
            IAuthenticationProvider authenticationProvider, 
            ISessionManager sessionManager,
            ISecurityDomainLogger logger)
        {
            _framework = framework;
            _metadataCache = metadataCache;
            _authenticationProvider = authenticationProvider;
            _sessionManager = sessionManager;
            _logger = logger;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Result Execute(string loginName, string password)
        {
            IApplicationDataRepository authenticationContext;
            IQueryable<IUserAccountEntity> userAccountQuery;
            
            using ( _sessionManager.JoinGlobalSystem() )
            {
                OpenAuthenticationContext(out authenticationContext, out userAccountQuery);
            }

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

                var session = _sessionManager.As<ICoreSessionManager>().AuthorieSession(principal);
                _logger.UserLoggedIn(principal.LoginName, principal.UserId, principal.EmailAddress, session.Id);

                var result = new Result(principal, currentSession.Endpoint, _metadataCache);
                return result;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void OpenAuthenticationContext(out IApplicationDataRepository context, out IQueryable<IUserAccountEntity> userAccounts)
        {
            var userAccountsContext = _framework.NewUnitOfWork<IUserAccountDataRepository>();

            context = userAccountsContext;
            userAccounts = userAccountsContext.AllUsers.AsQueryable();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void ExtendUserClaims(UserAccountPrincipal principal)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void ValidateUidlEndpointLogin(ISession currentSession, UserAccountPrincipal principal)
        {
            if ( currentSession == null )
            {
                return;
            }

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
            public Result(UserAccountPrincipal principal, IEndpoint endpoint, ITypeMetadataCache metadataCache)
            {
                var account = principal.Identity.GetUserAccount();

                FullName = principal.PersonFullName;
                UserType = metadataCache.GetTypeMetadata(account.As<IObject>().ContractType).QualifiedName;
                UserId = account.As<IPersistableObject>().As<IEntityObject>().GetId().Value.ToString();
                UserRoles = principal.GetUserRoles();
                AllClaims = principal.Identity.Claims.Select(c => c.Value).ToArray();
                LastLoginAtUtc = account.LastLoginAtUtc;

                if ( endpoint != null )
                {
                    IdleSessionExpiryMinutes = (int)endpoint.SessionIdleTimeoutDefault.GetValueOrDefault(TimeSpan.Zero).TotalMinutes;
                }

                var accountWithProfilePhoto = account as IEntityPartUserAccountProfilePhoto;

                if ( accountWithProfilePhoto != null && accountWithProfilePhoto.ProfilePhoto != null )
                {
                    this.ProfilePhotoId = EntityId.ValueOf(accountWithProfilePhoto.ProfilePhoto).ToStringOrDefault();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string FullName { get; private set; }
            public string UserId { get; private set; }
            public string UserType { get; private set; }
            public string[] UserRoles { get; private set; }
            public string[] AllClaims { get; private set; }
            public string ProfilePhotoId { get; private set; }
            public DateTime? LastLoginAtUtc { get; private set; }
            public int IdleSessionExpiryMinutes { get; private set; }
        }
    }
}
