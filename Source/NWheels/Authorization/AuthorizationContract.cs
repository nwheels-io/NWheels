using System;

namespace NWheels.Authorization
{
    public static class AuthorizationContract
    {
        [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Property)]
        public class AllowAnonymousAttribute : Attribute
        {
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Property)]
        public class RequireAttribute : Attribute
        {
            public RequireAttribute(params object[] claims)
            {
                this.Claims = claims;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public object[] Claims { get; private set; }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Property)]
        public class RequirePerResourceTypeAttribute : RequireAttribute
        {
            public RequirePerResourceTypeAttribute(Type resourceType, params object[] claims)
                : base(claims)
            {
                this.ResourceType = resourceType;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public Type ResourceType { get; private set; }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Property)]
        public class RequirePerResourceIdAttribute : RequirePerResourceTypeAttribute
        {
            public RequirePerResourceIdAttribute(Type resourceType, object resourceId, params object[] claims)
                : base(resourceType, claims)
            {
                this.ResourceId = resourceId;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public object ResourceId { get; private set; }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        [AttributeUsage(AttributeTargets.Enum)]
        public class ClaimsAttribute : Attribute
        {
        }
    }
}
