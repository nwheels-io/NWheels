namespace NWheels.Authorization.Claims
{
    public class OperationPermissionClaim : EnumClaimBase
    {
        public static readonly string OperationPermissionClaimTypeString = "OperationPermission";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public OperationPermissionClaim(object permissionEnumValue)
            : base(OperationPermissionClaimTypeString, GetEnumValueString(permissionEnumValue))
        {
        }
    }
}
