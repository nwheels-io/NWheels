using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NWheels.Authorization;
using NWheels.Domains.Security;
using NWheels.Entities;

namespace NWheels.Samples.MyHRApp.Authorization
{
    [EntityContract]
    [ClaimValue(HRClaims.AclAdministrator)]
    public interface IHRAdminAccessControlList : IEntityAccessRuleEntity
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class HRAdminAccessControlList : EntityAccessRuleEntity, IHRAdminAccessControlList
    {
        #region Overrides of EntityAccessRuleEntity

        public override void BuildAccessControl(IEntityAccessControlBuilder access)
        {
            access.ToAllEntities().IsDefinedHard(canRetrieve: true, canInsert: true, canUpdate: true, canDelete: false);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void InitializeProperties()
        {
            this.Name = "Administrator";
            this.Description = "Administrator";
            this.ClaimValue = HRClaims.AclAdministrator;
        }

        #endregion
    }
}