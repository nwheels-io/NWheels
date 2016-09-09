using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NWheels.Domains.Security;
using NWheels.Domains.Security.Core;
using NWheels.Entities;
using NWheels.Extensions;
using NWheels.Samples.MyHRApp.Authorization;
using NWheels.Samples.MyHRApp.Domain;
using NWheels.Utilities;

namespace NWheels.Samples.MyHRApp.Deployment
{
    public class HRContextPopulator : DomainContextPopulatorBase<IHRContext>
    {
        #region Overrides of DomainContextPopulatorBase<IHRContext>

        protected override void OnPopulateContext(IHRContext context)
        {
            var administratorAcl = AddUserDataRule<IHRAdminAccessControlLost>(context);
            var administratorRole = AddUserRole(context, "Administrator", HRClaims.UserRoleAdministrator, administratorAcl);
            var hrManagerRole = AddUserRole(context, "HR Manager", HRClaims.UserRoleHRManager, administratorAcl);

            var administratorUser = AddUser<IUserAccountEntity>(context, "admin", "Administrator", administratorRole);
            var helenBakerUser = AddUser<IUserAccountEntity>(context, "helen", "Helen Baker", hrManagerRole);

            var devDepartment = context.Departments.New();
            devDepartment.Name = "Research & Development";
            devDepartment.Manager = context.Employees.New();
            devDepartment.Manager.Name.FirstName = "James";
            devDepartment.Manager.Name.LastName = "Young";
            devDepartment.Manager.Email = "james.young@contoso.com";
            devDepartment.Manager.Phone = "(123) 123 1234";
            devDepartment.Manager.Address.StreetAddress = "123 First st";
            devDepartment.Manager.Address.City = "Metropolis";
            devDepartment.Manager.Address.ZipCode = "98765-4321";
            devDepartment.Manager.Address.Country = "United States";
            context.Employees.Insert(devDepartment.Manager);
            context.Departments.Insert(devDepartment);

            var qaDepartment = context.Departments.New();
            qaDepartment.Name = "Quality Assurance";
            qaDepartment.Manager = context.Employees.New();
            qaDepartment.Manager.Name.FirstName = "David";
            qaDepartment.Manager.Name.LastName = "King";
            qaDepartment.Manager.Email = "david.king@contoso.com";
            qaDepartment.Manager.Phone = "(123) 234 2345";
            qaDepartment.Manager.Address.StreetAddress = "234 Second st";
            qaDepartment.Manager.Address.City = "Metropolis";
            qaDepartment.Manager.Address.ZipCode = "98765-4322";
            qaDepartment.Manager.Address.Country = "United States";
            context.Employees.Insert(qaDepartment.Manager);
            context.Departments.Insert(qaDepartment);

            var opsDepartment = context.Departments.New();
            opsDepartment.Name = "Operations";
            opsDepartment.Manager = context.Employees.New();
            opsDepartment.Manager.Name.FirstName = "Roy";
            opsDepartment.Manager.Name.LastName = "Patterson";
            opsDepartment.Manager.Email = "roy.patterson@contoso.com";
            opsDepartment.Manager.Phone = "(123) 345 3456";
            opsDepartment.Manager.Address.StreetAddress = "345 Third st";
            opsDepartment.Manager.Address.City = "Metropolis";
            opsDepartment.Manager.Address.ZipCode = "98765-4323";
            opsDepartment.Manager.Address.Country = "United States";
            context.Employees.Insert(opsDepartment.Manager);
            context.Departments.Insert(opsDepartment);

            var marketingDepartment = context.Departments.New();
            marketingDepartment.Name = "Marketing";
            marketingDepartment.Manager = context.Employees.New();
            marketingDepartment.Manager.Name.FirstName = "Linda";
            marketingDepartment.Manager.Name.LastName = "Wood";
            marketingDepartment.Manager.Email = "linda.wood@contoso.com";
            marketingDepartment.Manager.Phone = "(123) 456 4567";
            marketingDepartment.Manager.Address.StreetAddress = "456 Fourth st";
            marketingDepartment.Manager.Address.City = "Metropolis";
            marketingDepartment.Manager.Address.ZipCode = "98765-4324";
            marketingDepartment.Manager.Address.Country = "United States";
            context.Employees.Insert(marketingDepartment.Manager);
            context.Departments.Insert(marketingDepartment);
        }

        #endregion
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static TUser AddUser<TUser>(IHRContext context, string loginName, string fullName, IUserRoleEntity role)
            where TUser : class, IUserAccountEntity
        {
            TUser user = context.AllUsers.AsQueryable().OfType<TUser>().FirstOrDefault(x => x.LoginName == loginName);

            if (user == null)
            {
                user = context.AllUsers.New<TUser>();

                user.LoginName = loginName;
                user.FullName = fullName;
                user.AssociatedRoles.Add(role);
                user.As<UserAccountEntity>().SetPassword(SecureStringUtility.ClearToSecure("11111"));
                user.As<IActiveRecord>().Save();
            }
            else if (!user.AssociatedRoles.Contains(role))
            {
                user.AssociatedRoles.Clear();
                user.AssociatedRoles.Add(role);
                user.As<IActiveRecord>().Save();
            }
            return user;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal static IUserRoleEntity AddUserRole(
            IUserAccountDataRepository context,
            string title,
            string claimValue,
            params IEntityAccessRuleEntity[] entityAccessRules)
        {
            IUserRoleEntity role = context.UserRoles.AsQueryable().FirstOrDefault(x => x.ClaimValue == claimValue);

            if (role == null)
            {
                role = context.UserRoles.New();

                role.Name = title;
                role.ClaimValue = claimValue;

                foreach (var rule in entityAccessRules)
                {
                    role.AssociatedEntityAccessRules.Add(rule);
                }

                context.UserRoles.Insert(role);
            }

            return role;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static IOperationPermissionEntity AddUserPermission(IUserAccountDataRepository context, string permissionName)
        {
            string pascalName = permissionName.SplitPascalCase();
            IOperationPermissionEntity permission = context.OperationPermissions.AsQueryable().FirstOrDefault(x => x.Name == pascalName);

            if (permission == null)
            {
                permission = context.OperationPermissions.New();

                permission.Name = pascalName;
                permission.ClaimValue = permissionName;

                context.OperationPermissions.Insert(permission);
            }

            return permission;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal static IEntityAccessRuleEntity AddUserDataRule<TRule>(IUserAccountDataRepository context) where TRule : class, IEntityAccessRuleEntity
        {
            var rule = context.EntityAccessRules.AsQueryable().ToList().OfType<TRule>().FirstOrDefault();
            if (rule == null)
            {
                rule = context.EntityAccessRules.New<TRule>();
                context.EntityAccessRules.Insert(rule);
            }
            return rule;
        }
    }
}