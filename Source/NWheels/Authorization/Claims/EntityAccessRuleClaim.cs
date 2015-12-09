using System.Security.Claims;

namespace NWheels.Authorization.Claims
{
    public class EntityAccessRuleClaim : Claim
    {
        public static readonly string EntityAccessRuleClaimTypeString = "EntityAccessRule";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EntityAccessRuleClaim(string claimValue)
            : base(EntityAccessRuleClaimTypeString, claimValue)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EntityAccessRuleClaim(IEntityAccessRule rule, string claimValue)
            : base(EntityAccessRuleClaimTypeString, claimValue)
        {
            this.Rule = rule;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEntityAccessRule Rule { get; protected set; }
    }
}
