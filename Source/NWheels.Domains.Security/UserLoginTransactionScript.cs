using System;
using System.Linq;
using NWheels.Authorization.Core;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.Domains.Security.Core;
using NWheels.Entities.Core;
using NWheels.Extensions;
using NWheels.Processing;
using NWheels.Utilities;

namespace NWheels.Domains.Security
{
    public class UserLoginTransactionScript : ITransactionScript
    {
        private readonly IAuthenticationProvider _authenticationProvider;
        private readonly ICoreSessionManager _sessionManager;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UserLoginTransactionScript(IAuthenticationProvider authenticationProvider, ICoreSessionManager sessionManager)
        {
            _authenticationProvider = authenticationProvider;
            _sessionManager = sessionManager;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Result Execute(
            [PropertyContract.Semantic.LoginName] 
            string loginName,
            [PropertyContract.Semantic.Password] 
            string password)
        {
            IUserAccountEntity userAccount;
            
            var principal = _authenticationProvider.Authenticate(loginName, SecureStringUtility.ClearToSecure(password), out userAccount);
            _sessionManager.AuthorieSession(principal);

            var result = new Result(principal);
            return result;
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
