using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NWheels.Authorization;
using NWheels.Authorization.Core;
using NWheels.DataObjects;
using NWheels.Domains.Security;
using NWheels.Entities;
using NWheels.Extensions;
using NWheels.Testing;

namespace NWheels.Stacks.ODataBreeze.SystemTests
{
    public static class TestData
    {
        public static class SecurityDomain
        {
            public static void InsertBasic(IFramework framework)
            {
                using ( var data1 = framework.NewUnitOfWork<IUserAccountDataRepository>() )
                {
                    AddUserRole(data1, TestClaims.RoleOne);
                    AddUserRole(data1, TestClaims.RoleTwo);
                    AddUserRole(data1, TestClaims.RoleThree);

                    AddUserPermission(data1, TestClaims.PermissionOne);
                    AddUserPermission(data1, TestClaims.PermissionTwo);
                    AddUserPermission(data1, TestClaims.PermissionThree);

                    AddUserDataRule(data1, TestClaims.DataRuleOne);

                    CreateUsers(data1);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static void CreateUsers(IUserAccountDataRepository data1)
            {
                //var admin1 = data1.AllUsers.New<IBackEndUserAccountEntity>();
                //admin1.LoginName = "admin";
                //admin1.FullName = "Administrator";
                //var adminPass1 = data1.NewPassword();
                //adminPass1.Hash = Encoding.ASCII.GetBytes("adminPass1");
                //admin1.Passwords.Add(adminPass1);
                //admin1.AssociatedRoles = new[] { TestClaims.RoleOne };
                //data1.AllUsers.Insert(admin1);

                //var subscriber1 = data1.AllUsers.New<IFrontEndUserAccountEntity>();
                //subscriber1.LoginName = "johns";
                //subscriber1.FullName = "John Smith";
                //subscriber1.EmailAddress = "john.smith@email.com";
                //var subscriberPassword1 = data1.NewPassword();
                //subscriberPassword1.Hash = Encoding.ASCII.GetBytes("johnsPass1");
                //subscriber1.Passwords.Add(subscriberPassword1);
                //subscriber1.AssociatedRoles = new[] { TestClaims.RoleTwo, TestClaims.RoleThree };
                //data1.AllUsers.Insert(subscriber1);

                data1.CommitChanges();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static void AddUserRole(IUserAccountDataRepository data, string value)
            {
                var role = data.UserRoles.New();

                role.Name = value.ToString().SplitPascalCase();
                role.ClaimValue = value;

                data.UserRoles.Insert(role);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static void AddUserPermission(IUserAccountDataRepository data, string value)
            {
                var permission = data.OperationPermissions.New();

                permission.Name = value.ToString().SplitPascalCase();
                permission.ClaimValue = value;

                data.OperationPermissions.Insert(permission);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static void AddUserDataRule(IUserAccountDataRepository data, string value)
            {
                var rule = data.EntityAccessRules.New();

                rule.Name = value.ToString().SplitPascalCase();
                rule.ClaimValue = value;
                //rule.RuleObject = typeof(TestDataRuleOne);

                data.EntityAccessRules.Insert(rule);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static class TestClaims
            {
                public const string RoleOne = "RoleOne";
                public const string RoleTwo = "RoleTwo";
                public const string RoleThree = "RoleThree";
                public const string PermissionOne = "PermissionOne";
                public const string PermissionTwo = "PermissionTwo";
                public const string PermissionThree = "PermissionThree";
                public const string DataRuleOne = "DataRuleOne";
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public class TestDataRuleOne : IEntityAccessRule
            {
                #region Implementation of IEntityAccessRule

                public void BuildAccessControl(IEntityAccessControlBuilder access)
                {
                    access.ToEntity<IUserAccountEntity>()
                        .IsDefinedHard(
                            canInsert: false, 
                            canDelete: false)
                        .IsDefinedByPredicate(
                            canRetrieve: (context, user) => context.Session.GetUserAccountAs<IUserAccountEntity>() == user,
                            canUpdate: (context, user) => context.Session.GetUserAccountAs<IUserAccountEntity>() == user);
                }

                #endregion
            }
        }
    }
}
