using System.Security.Claims;

namespace NWheels.Authorization.Claims
{
    public class EntityAccessRuleClaim : Claim
    {
        public static readonly string EntityAccessRuleClaimTypeString = "EntityAccessRule";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private readonly IEntityAccessRule _rule;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EntityAccessRuleClaim(IEntityAccessRule rule, string claimValue)
            : base(EntityAccessRuleClaimTypeString, claimValue)
        {
            _rule = rule;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEntityAccessRule Rule
        {
            get { return _rule; }
        }
    }
}
