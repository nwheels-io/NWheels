using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NWheels.Authorization;
using NWheels.Domains.Security;
using NWheels.Entities;

namespace NWheels.Samples.MyMusicDB.Authorization
{
    [EntityContract]
    [ClaimValue(MusicDBClaims.AclAdministrator)]
    public interface IAdministratorAcl : IEntityAccessRuleEntity
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class AdministratorAcl : EntityAccessRuleEntity, IAdministratorAcl
    {
        #region Overrides of EntityAccessRuleEntity

        public override void BuildAccessControl(IEntityAccessControlBuilder access)
        {
            access.ToAllEntities().IsDefinedHard(canRetrieve: true, canInsert: true, canUpdate: true, canDelete: true);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void InitializeProperties()
        {
            this.Name = "Administrator";
            this.Description = "Administrator";
            this.ClaimValue = MusicDBClaims.AclAdministrator;
        }

        #endregion
    }
}
