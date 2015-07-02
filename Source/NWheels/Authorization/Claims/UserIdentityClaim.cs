using System;
using System.Security.Claims;

namespace NWheels.Authorization.Claims
{
    public abstract class UserIdentityClaim : Claim, IIdentityInfo
    {
        public static readonly string UserIdentityClaimTypeString = "UserIdentity";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected UserIdentityClaim()
            : base(UserIdentityClaimTypeString, value: string.Empty)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract bool IsOfType(Type accountEntityType);
        public abstract string LoginName { get; }
        public abstract string QualifiedLoginName{ get; }
        public abstract string PersonFullName { get; }
        public abstract string EmailAddress { get; }
        public abstract string AuthenticationType { get; }
        public abstract bool IsAuthenticated { get; }
        public abstract string Name { get; }
    }
}
