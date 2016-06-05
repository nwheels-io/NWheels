using NWheels.Authorization.Claims;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Claims;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Threading;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.Authorization;
using NWheels.Authorization.Core;
using NWheels.Conventions.Core;
using NWheels.Exceptions;
using System.Security.Principal;

namespace NWheels
{
    public static class SecurityCheck
    {
        private static readonly System.Collections.Hashtable _s_claimValueByEntityAccessRuleContract = new System.Collections.Hashtable();
        private static readonly object _s_claimValueWriterSyncRoot = new object();

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public static void RequireAuthentication()
        {
            RequireAuthentication(Thread.CurrentPrincipal);
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public static void RequireUserRole(object userRole)
        {
            RequireUserRole(Thread.CurrentPrincipal, userRole);
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public static void RequirePermission(object permission)
        {
            RequirePermission(Thread.CurrentPrincipal, permission);
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public static void RequireEntityAccessRule(Type entityAccessRuleType)
        {
            RequireEntityAccessRule(Thread.CurrentPrincipal, entityAccessRuleType);
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public static void RequireClaim(IPrincipal principal, string claimType, object claimValue)
        {
            if ( principal is SystemPrincipal )
            {
                return;
            }

            var claimsPrincipal = principal as ClaimsPrincipal;

            if ( claimsPrincipal == null )
            {
                throw new AccessDeniedException();
            }

            if ( !claimsPrincipal.HasClaim(claimType, claimValue.ToString()) )
            {
                throw new AccessDeniedException(claimType, claimValue.ToString());
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public static void RequireAuthentication(IPrincipal principal)
        {
            if ( principal == null || principal.Identity == null || !principal.Identity.IsAuthenticated )
            {
                throw new AccessDeniedException();
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public static void RequireUserRole(IPrincipal principal, object userRole)
        {
            if ( principal == null || !principal.IsInRole(userRole.ToString()) )
            {
                throw new AccessDeniedException();
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public static void RequirePermission(IPrincipal principal, object permission)
        {
            RequireClaim(principal, OperationPermissionClaim.OperationPermissionClaimTypeString, permission);
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public static void RequireEntityAccessRule(IPrincipal principal, Type entityAccessRuleType)
        {
            var claimValue = GetEntityAccessRuleClaimValue(entityAccessRuleType);
            RequireClaim(principal, EntityAccessRuleClaim.EntityAccessRuleClaimTypeString, claimValue);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string GetEntityAccessRuleClaimValue(Type entityAccessRuleType)
        {
            // ReSharper disable once InconsistentlySynchronizedField (Hashtable is safe for concurrent reads)
            var existingValue = _s_claimValueByEntityAccessRuleContract[entityAccessRuleType]; 

            if ( existingValue != null )
            {
                return (string)existingValue;
            }

            var claimValueAttribute = entityAccessRuleType.GetCustomAttribute<ClaimValueAttribute>();
            var newValue = (claimValueAttribute != null ? claimValueAttribute.Value : entityAccessRuleType.FullName);

            lock ( _s_claimValueWriterSyncRoot )
            {
                _s_claimValueByEntityAccessRuleContract[entityAccessRuleType] = newValue;
            }

            return newValue;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class AllowAnonymousAttribute : AccessSecurityAttributeBase
        {
            #region Overrides of AccessSecurityAttributeBase

            public override void ValidateAccessOrThrow(ClaimsPrincipal principal)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void WriteSecurityCheck(MethodWriterBase writer, IOperand<ClaimsPrincipal> principal, StaticStringsDecorator staticStrings)
            {
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static void WriteRequireAuthenticationCheck(MethodWriterBase writer, IOperand<ClaimsPrincipal> principal)
            {
                Static.Void(RequireAuthentication, principal);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class RequireUserRoleAttribute : AccessSecurityAttributeBase
        {
            private readonly object _userRole;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public RequireUserRoleAttribute(object userRole)
            {
                _userRole = userRole;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of AccessSecurityAttributeBase

            public override void ValidateAccessOrThrow(ClaimsPrincipal principal)
            {
                RequireUserRole(principal, _userRole);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void WriteSecurityCheck(MethodWriterBase writer, IOperand<ClaimsPrincipal> principal, StaticStringsDecorator staticStrings)
            {
                Static.Void(RequireUserRole, principal, staticStrings.GetStaticStringOperand(_userRole.ToString()));
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class RequirePermissionAttribute : AccessSecurityAttributeBase
        {
            private readonly object _permission;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public RequirePermissionAttribute(object permission)
            {
                _permission = permission;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of AccessSecurityAttributeBase

            public override void ValidateAccessOrThrow(ClaimsPrincipal principal)
            {
                RequirePermission(principal, _permission);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void WriteSecurityCheck(MethodWriterBase writer, IOperand<ClaimsPrincipal> principal, StaticStringsDecorator staticStrings)
            {
                Static.Void(RequirePermission, principal, staticStrings.GetStaticStringOperand(_permission.ToString()));
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class RequireEntityAccessRuleAttribute : AccessSecurityAttributeBase
        {
            private readonly Type _ruleContractType;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public RequireEntityAccessRuleAttribute(Type ruleContractType)
            {
                _ruleContractType = ruleContractType;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of AccessSecurityAttributeBase

            public override void ValidateAccessOrThrow(ClaimsPrincipal principal)
            {
                RequireEntityAccessRule(principal, _ruleContractType);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void WriteSecurityCheck(MethodWriterBase writer, IOperand<ClaimsPrincipal> principal, StaticStringsDecorator staticStrings)
            {
                Static.Void(RequireEntityAccessRule, principal, writer.Const(_ruleContractType));
            }

            #endregion
        }
    }
}
