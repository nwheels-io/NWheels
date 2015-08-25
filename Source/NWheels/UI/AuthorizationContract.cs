using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    public static class AuthorizationContract
    {
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
        public abstract class AuthorizationAttribute : Attribute
        {
            public string[] UserRoles { get; protected set; }
            public string[] Permissions { get; protected set; }
            public string[] EntityAccessRules { get; protected set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class UserRolesAttribute : AuthorizationAttribute
        {
            public UserRolesAttribute(params string[] userRoles)
            {
                base.UserRoles = userRoles;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class PermissionsAttribute : AuthorizationAttribute
        {
            public PermissionsAttribute(params string[] permissions)
            {
                base.Permissions = permissions;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class EntityAccessRulesAttribute : AuthorizationAttribute
        {
            public EntityAccessRulesAttribute(params string[] rules)
            {
                base.EntityAccessRules = rules;
            }
        }
    }
}
