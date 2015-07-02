namespace NWheels.Authorization.Claims
{
    public class EntityAccessRuleClaim : EnumClaimBase
    {
        public static readonly string EntityAccessRuleClaimTypeString = "EntityAccessRule";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EntityAccessRuleClaim(object permissionEnumValue)
            : base(EntityAccessRuleClaimTypeString, GetEnumValueString(permissionEnumValue))
        {
        }
    }
}
