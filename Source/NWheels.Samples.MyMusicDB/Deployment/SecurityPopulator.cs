using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using NWheels.Domains.Security;
using NWheels.Domains.Security.Core;
using NWheels.Entities;
using NWheels.Extensions;
using NWheels.Processing.Documents;
using NWheels.Samples.MyMusicDB.Authorization;
using NWheels.Samples.MyMusicDB.Domain;
using NWheels.UI;
using NWheels.Utilities;

namespace NWheels.Samples.MyMusicDB.Deployment
{
    public class SecurityPopulator : DomainContextPopulatorBase<IMusicDBContext>
    {
        private readonly IFrameworkUIConfig _uiConfig;

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public SecurityPopulator(IFrameworkUIConfig uiConfig)
        {
            _uiConfig = uiConfig;
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of DomainContextPopulatorBase<IHRContext>

        protected override void OnPopulateContext(IMusicDBContext context)
        {
            var administratorAcl = AddUserDataRule<IAdministratorAcl>(context);
            var administratorRole = AddUserRole(context, "Administrator", MusicDBClaims.UserRoleAdministrator, administratorAcl);

            var operatorAcl = AddUserDataRule<IOperatorAcl>(context);
            var operatorRole = AddUserRole(context, "Operator", MusicDBClaims.UserRoleOperator, operatorAcl);

            AddUser<IMusicDBUserAccountEntity>(context, "admin", "Administrator", administratorRole);

            var helenBakerPhotoPath = PathUtility.HostBinPath(_uiConfig.WebContentRootPath, @"skin.inspinia\assets\img\a3_48.jpg");
            AddUser<IMusicDBUserAccountEntity>(context, "helen", "Helen Baker", operatorRole, helenBakerPhotoPath);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static TUser AddUser<TUser>(IMusicDBContext context, string loginName, string fullName, IUserRoleEntity role, string profilePhotoPath = null)
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

                if (profilePhotoPath != null)
                {
                    var profilePhoto = AddProfilePhoto<TUser>(context, profilePhotoPath);
                    user.As<IEntityPartUserAccountProfilePhoto>().ProfilePhoto = profilePhoto;
                }

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

        private static IProfilePhotoEntity AddProfilePhoto<TUser>(IMusicDBContext context, string profilePhotoPath) where TUser : class, IUserAccountEntity
        {
            var imageType = Path.GetExtension(profilePhotoPath).TrimStart('.').ToLower();
            if (imageType == "jpg")
            {
                imageType = "jpeg";
            }
            var imageFormat = new DocumentFormat("IMAGE", "image/" + imageType, Path.GetExtension(profilePhotoPath), Path.GetFileName(profilePhotoPath));
            var imageDocument = new FormattedDocument(new DocumentMetadata(imageFormat), File.ReadAllBytes(profilePhotoPath));
            var profilePhoto = context.ProfilePhotos.New();
            profilePhoto.ImportFormattedDocument(imageDocument);
            context.ProfilePhotos.Insert(profilePhoto);
            return profilePhoto;
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
                role.Description = title;

                foreach (var rule in entityAccessRules)
                {
                    role.AssociatedEntityAccessRules.Add(rule);
                }

                context.UserRoles.Insert(role);
            }

            return role;
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