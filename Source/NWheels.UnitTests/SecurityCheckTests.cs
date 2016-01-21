using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using NWheels.Authorization;
using NWheels.Authorization.Claims;
using NWheels.Exceptions;
using NWheels.Testing;
using Shouldly;

namespace NWheels.UnitTests
{
    [TestFixture]
    public class SecurityCheckTests : UnitTestBase
    {
        [Test]
        public void RequireAuthentication_NoPrincipal_Fail()
        {
            Thread.CurrentPrincipal = null;

            Should.Throw<AccessDeniedException>(() => {
                SecurityCheck.RequireAuthentication();        
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void RequireAuthentication_AnonymousPrincipal_Fail()
        {
            Thread.CurrentPrincipal = new AnonymousPrincipal(Framework.MetadataCache, Framework.Logger<IAuthorizationLogger>(), new AnonymousEntityAccessRule[0]);

            Should.Throw<AccessDeniedException>(() => {
                SecurityCheck.RequireAuthentication();
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void RequireAuthentication_SystemPrincipal_Pass()
        {
            Thread.CurrentPrincipal = new SystemPrincipal();
            SecurityCheck.RequireAuthentication();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void RequireAuthentication_AuthenticatedUser_Pass()
        {
            Thread.CurrentPrincipal = new AuthenticatedPrincipal();
            SecurityCheck.RequireAuthentication();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void RequireUserRole_NoPrincipal_Fail()
        {
            Thread.CurrentPrincipal = null;

            Should.Throw<AccessDeniedException>(() => {
                SecurityCheck.RequireUserRole("R1");
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void RequireUserRole_AnonymousPrincipal_Fail()
        {
            Thread.CurrentPrincipal = new AnonymousPrincipal(Framework.MetadataCache, Framework.Logger<IAuthorizationLogger>(), new AnonymousEntityAccessRule[0]);

            Should.Throw<AccessDeniedException>(() => {
                SecurityCheck.RequireUserRole("R1");
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void RequireUserRole_SystemPrincipal_Pass()
        {
            Thread.CurrentPrincipal = new SystemPrincipal();
            SecurityCheck.RequireUserRole("R1");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void RequireUserRole_UserHasRole_Pass()
        {
            Thread.CurrentPrincipal = new AuthenticatedPrincipal(userRoles: new[] { "R1" });
            SecurityCheck.RequireUserRole("R1");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void RequireUserRole_UserHasNoRole_Fail()
        {
            Thread.CurrentPrincipal = new AuthenticatedPrincipal(userRoles: new[] { "R2" });

            Should.Throw<AccessDeniedException>(() =>{
                SecurityCheck.RequireUserRole("R1");
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void RequirePermission_NoPrincipal_Fail()
        {
            Thread.CurrentPrincipal = null;

            Should.Throw<AccessDeniedException>(() => {
                SecurityCheck.RequirePermission("P1");
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void RequirePermission_AnonymousPrincipal_Fail()
        {
            Thread.CurrentPrincipal = new AnonymousPrincipal(Framework.MetadataCache, Framework.Logger<IAuthorizationLogger>(), new AnonymousEntityAccessRule[0]);

            Should.Throw<AccessDeniedException>(() => {
                SecurityCheck.RequirePermission("P1");
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void RequirePermission_SystemPrincipal_Pass()
        {
            Thread.CurrentPrincipal = new SystemPrincipal();
            SecurityCheck.RequirePermission("P1");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void RequirePermission_UserHasPermission_Pass()
        {
            Thread.CurrentPrincipal = new AuthenticatedPrincipal(permissions: new[] { "P1" });
            SecurityCheck.RequirePermission("P1");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void RequirePermission_UserHasNoPermission_Fail()
        {
            Thread.CurrentPrincipal = new AuthenticatedPrincipal(permissions: new[] { "P2" });

            Should.Throw<AccessDeniedException>(() => {
                SecurityCheck.RequirePermission("P1");
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void RequireEntityAccessRule_NoPrincipal_Fail()
        {
            Thread.CurrentPrincipal = null;

            Should.Throw<AccessDeniedException>(() => {
                SecurityCheck.RequireEntityAccessRule(typeof(ITestEntityRuleOne));
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void RequireEntityAccessRule_AnonymousPrincipal_Fail()
        {
            Thread.CurrentPrincipal = new AnonymousPrincipal(Framework.MetadataCache, Framework.Logger<IAuthorizationLogger>(), new AnonymousEntityAccessRule[0]);

            Should.Throw<AccessDeniedException>(() => {
                SecurityCheck.RequireEntityAccessRule(typeof(ITestEntityRuleOne));
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void RequireEntityAccessRule_SystemPrincipal_Pass()
        {
            Thread.CurrentPrincipal = new SystemPrincipal();
            SecurityCheck.RequireEntityAccessRule(typeof(ITestEntityRuleOne));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void RequireEntityAccessRule_UserHasRule_Pass()
        {
            Thread.CurrentPrincipal = new AuthenticatedPrincipal(entityAccessRules: new[] { "TestRuleOne" });
            SecurityCheck.RequireEntityAccessRule(typeof(ITestEntityRuleOne));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void RequireEntityAccessRule_UserHasNoRule_Fail()
        {
            Thread.CurrentPrincipal = new AuthenticatedPrincipal(entityAccessRules: new[] { "TestRuleTwo" });

            Should.Throw<AccessDeniedException>(() => {
                SecurityCheck.RequireEntityAccessRule(typeof(ITestEntityRuleOne));
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void RequireEntityAccessRuleWithDefaultClaimValue_UserHasRule_Pass()
        {
            Thread.CurrentPrincipal = new AuthenticatedPrincipal(entityAccessRules: new[] { "NWheels.UnitTests.SecurityCheckTests+ITestEntityRuleTwo" });
            SecurityCheck.RequireEntityAccessRule(typeof(ITestEntityRuleTwo));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void RequireEntityAccessRuleWithDefaultClaimValue_UserHasNoRule_Fail()
        {
            Thread.CurrentPrincipal = new AuthenticatedPrincipal(entityAccessRules: new[] { "TestRuleTwo" });

            Should.Throw<AccessDeniedException>(() => {
                SecurityCheck.RequireEntityAccessRule(typeof(ITestEntityRuleTwo));
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class AuthenticatedPrincipal : ClaimsPrincipal
        {
            public AuthenticatedPrincipal(string[] userRoles = null, string[] permissions = null, string[] entityAccessRules = null)
                : base(new AuthenticatedIdentity(userRoles, permissions, entityAccessRules))
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override bool IsInRole(string role)
            {
                return base.HasClaim(c => c.Type == UserRoleClaim.UserRoleClaimTypeString && c.Value == role);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class AuthenticatedIdentity : ClaimsIdentity
        {
            public AuthenticatedIdentity(string[] userRoles = null, string[] permissions = null, string[] entityAccessRules = null)
                : base(CreateClaims(userRoles, permissions, entityAccessRules))
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override string AuthenticationType
            {
                get { return "TEST"; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override bool IsAuthenticated
            {
                get { return true; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override string Name
            {
                get { return "user1"; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static IEnumerable<Claim> CreateClaims(string[] userRoles, string[] permissions, string[] entityAccessRules)
            {
                var claims = new List<Claim>();

                if ( userRoles != null )
                {
                    claims.AddRange(userRoles.Select(s => new UserRoleClaim(s)));
                }

                if ( permissions != null )
                {
                    claims.AddRange(permissions.Select(s => new OperationPermissionClaim(s)));
                }

                if ( entityAccessRules != null )
                {
                    claims.AddRange(entityAccessRules.Select(s => new EntityAccessRuleClaim(s)));
                }

                return claims;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ClaimValue("TestRuleOne")]
        private interface ITestEntityRuleOne : IEntityAccessRule
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private interface ITestEntityRuleTwo : IEntityAccessRule
        {
        }
    }
}
