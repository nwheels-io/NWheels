using System;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using System.Threading;
using NUnit.Framework;

namespace NWheels.Domains.Security.Tests.Unit
{
    [TestFixture]
    public class AssumptionTests
    {
        [Test, ExpectedException(typeof(SecurityException), ExpectedMessage = "No principal present")]
        public void CallSecuredMethod_NoPrincipal_Throw()
        {
            MethodWhichRequiresSalePermission();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test, ExpectedException(typeof(SecurityException), ExpectedMessage = "Principal has no [PERMISSION:ExecuteSale]")]
        public void CallSecuredMethod_PrincipalHasNoPermission_Throw()
        {
            Thread.CurrentPrincipal = new TestPrincipal(new AuthenticatedTestIdentity("user1", "PERMISSION:Abc"));
            MethodWhichRequiresSalePermission();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CallSecuredMethod_PrincipalHasPermission_Pass()
        {
            Thread.CurrentPrincipal = new TestPrincipal(new AuthenticatedTestIdentity("user1", "PERMISSION:Abc", "PERMISSION:ExecuteSale"));
            MethodWhichRequiresSalePermission();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test, ExpectedException(typeof(SecurityException), ExpectedMessage = "No principal present")]
        public void InstantiateSecuredClass_NoPrincipal_Throw()
        {
            var obj = new ClassWhichRequiresAdminRole(prefix: "PPP");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test, ExpectedException(typeof(SecurityException), ExpectedMessage = "Principal has no [ROLE:Admin]")]
        public void InstantiateSecuredClass_PrincipalHasNoPermission_Throw()
        {
            Thread.CurrentPrincipal = new TestPrincipal(new AuthenticatedTestIdentity("user1", "PERMISSION:Abc"));
            var obj = new ClassWhichRequiresAdminRole(prefix: "PPP");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void InstantiateSecuredClass_PrincipalHasPermission_Pass()
        {
            Thread.CurrentPrincipal = new TestPrincipal(new AuthenticatedTestIdentity("user1", "PERMISSION:Abc", "ROLE:Admin"));
            var obj = new ClassWhichRequiresAdminRole(prefix: "PPP");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ExecuteSalePermission(SecurityAction.Demand)]
        private void MethodWhichRequiresSalePermission()
        {
            Console.WriteLine("MethodWhichRequiresSalePermission: ACCESS GRANTED!!!");            
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public enum TestDataPermission
        {
            Create,
            Retrieve,
            Update,
            Delete
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public enum TestActionPermission
        {
            ExecuteSale,
            ViewSaleReport,
            SendEmailToCustomer
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public enum TestUserRole
        {
            Customer,
            SalePerson,
            Manager,
            Admin
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class AdminUserRoleAttribute : TestUserRoleAttribute
        {
            public AdminUserRoleAttribute(SecurityAction action)
                : base(action, TestUserRole.Admin)
            {
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ExecuteSalePermissionAttribute : TestUserPermissionAttribute
        {
            public ExecuteSalePermissionAttribute(SecurityAction action)
                : base(action, TestActionPermission.ExecuteSale)
            {
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [AdminUserRole(SecurityAction.Demand)]
        private class ClassWhichRequiresAdminRole
        {
            private readonly string _prefix;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ClassWhichRequiresAdminRole(string prefix)
            {
                _prefix = prefix;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string FirstMethod()
            {
                return _prefix + "111";
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            public string SecondMethod()
            {
                return _prefix + "222";
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestPrincipal : IPrincipal
        {
            private readonly TestIdentityBase _identity;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TestPrincipal(TestIdentityBase identity)
            {
                _identity = identity;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IIdentity Identity
            {
                get { return _identity; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool HasClaim(string claim)
            {
                return _identity.HasClaim(claim);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool IsInRole(string role)
            {
                return _identity.HasClaim("ROLE:" + role);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract class TestIdentityBase : IIdentity
        {
            private readonly string[] _claims;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected TestIdentityBase(params string[] claims)
            {
                _claims = claims;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool HasClaim(string claim)
            {
                return _claims.Contains(claim);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public abstract string AuthenticationType { get; }
            public abstract bool IsAuthenticated { get; }
            public abstract string Name { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class AnonymousTestIdentity : TestIdentityBase
        {
            public AnonymousTestIdentity(params string[] claims)
                : base(claims.Concat(new[] { "ANONYMOUS" }).ToArray())
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override string AuthenticationType
            {
                get { return null; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override bool IsAuthenticated
            {
                get { return false; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override string Name
            {
                get { return null; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class AuthenticatedTestIdentity : TestIdentityBase
        {
            private readonly string _userName;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public AuthenticatedTestIdentity(string userName, params string[] claims)
                : base(claims.Concat(new[] { "AUTHENTICATED" }).ToArray())
            {
                _userName = userName;
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
                get { return _userName; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract class TestClaimsPermissionAttribute : CodeAccessSecurityAttribute
        {
            private readonly string[] _requiredClaims;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected TestClaimsPermissionAttribute(SecurityAction action, params string[] requiredClaims)
                : base(SecurityAction.Demand)
            {
                _requiredClaims = requiredClaims;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override IPermission CreatePermission()
            {
                return new TestClaimsPermission(_requiredClaims);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestUserRoleAttribute : TestClaimsPermissionAttribute
        {
            public TestUserRoleAttribute(SecurityAction action, params object[] userRoles)
                : base(action, userRoles.Select(r => "ROLE:" + r.ToString()).ToArray())
            {
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestUserPermissionAttribute : TestClaimsPermissionAttribute
        {
            public TestUserPermissionAttribute(SecurityAction action, params object[] permissions)
                : base(action, permissions.Select(p => "PERMISSION:" + p.ToString()).ToArray())
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TestUserPermissionAttribute(SecurityAction action, Type resourceType, params object[] permissions)
                : base(action, permissions.Select(p => "PERMISSION:" + p.ToString() + "[" + resourceType.Name + "]").ToArray())
            {

            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestClaimsPermission : IPermission
        {
            private readonly Type _resouceType;
            private readonly string[] _requiredClaims;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TestClaimsPermission(string[] requiredClaims)
            {
                _resouceType = null;
                _requiredClaims = requiredClaims;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TestClaimsPermission(Type resouceType, string[] requiredClaims)
            {
                _resouceType = resouceType;
                _requiredClaims = requiredClaims;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IPermission Copy()
            {
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Demand()
            {
                var principal = Thread.CurrentPrincipal as TestPrincipal;

                if ( principal == null )
                {
                    throw new SecurityException("No principal present");
                }

                var failedClaim = _requiredClaims.FirstOrDefault(c => !principal.HasClaim(c));
                
                if ( failedClaim != null )
                {
                    throw new SecurityException(string.Format("Principal has no [{0}]", failedClaim));
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IPermission Intersect(IPermission target)
            {
                return target;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool IsSubsetOf(IPermission target)
            {
                return false;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IPermission Union(IPermission target)
            {
                return target;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void FromXml(SecurityElement e)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public SecurityElement ToXml()
            {
                throw new NotImplementedException();
            }
        }
    }
}
