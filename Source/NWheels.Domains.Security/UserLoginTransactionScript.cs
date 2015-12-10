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

namespace NWheels.Domains.Security
{
    public class UserLoginTransactionScript : ITransactionScript
    {
        private readonly IAuthenticationProvider _authenticationProvider;
        private readonly ISessionManager _sessionManager;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UserLoginTransactionScript(IAuthenticationProvider authenticationProvider, ISessionManager sessionManager)
        {
            _authenticationProvider = authenticationProvider;
            _sessionManager = sessionManager;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Result Execute(string loginName, string password)
        {
            IUserAccountEntity userAccount;
            UserAccountPrincipal principal;

            var currentSession = _sessionManager.CurrentSession;

            using ( _sessionManager.JoinGlobalSystem() )
            {
                principal = _authenticationProvider.Authenticate(loginName, SecureStringUtility.ClearToSecure(password), out userAccount);
            }

            AttachExtendedClaims(principal);

            var uidlEndpoint = currentSession.Endpoint as IUidlApplicationEndpoint;

            if ( uidlEndpoint != null )
            {
                if ( !uidlEndpoint.UidlApplication.Authorization.TryValidateUser(principal.Identity) )
                {
                    throw new DomainFaultException<LoginFault>(LoginFault.NotAuthorized);
                }
            }

            if ( currentSession.IsGlobalImmutable )
            {
                _sessionManager.OpenAnonymous(currentSession.Endpoint);
            }
            
            _sessionManager.As<ICoreSessionManager>().AuthorieSession(principal);

            var result = new Result(principal);
            return result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public event EventHandler<ExtendedClaimsEventArgs> AttachingExtendedClaims;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AttachExtendedClaims(UserAccountPrincipal principal)
        {
            if ( AttachingExtendedClaims != null )
            {
                var args = new ExtendedClaimsEventArgs();
                AttachingExtendedClaims(this, args);
                principal.Identity.ExtendClaimsOnce(args.ExtendedClaims);
            }

            principal.Identity.DoneExtendingClaims();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ExtendedClaimsEventArgs : EventArgs
        {
            public ExtendedClaimsEventArgs()
            {
                ExtendedClaims = new List<Claim>();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public List<Claim> ExtendedClaims { get; private set; }
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class Result
        {
            internal Result(UserAccountPrincipal principal)
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
