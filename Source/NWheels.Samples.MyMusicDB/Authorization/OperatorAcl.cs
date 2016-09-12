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
    public interface IOperatorAcl : IEntityAccessRuleEntity
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class OperatorAcl : EntityAccessRuleEntity, IOperatorAcl
    {
        #region Overrides of EntityAccessRuleEntity

        public override void BuildAccessControl(IEntityAccessControlBuilder access)
        {
            access.ToAllEntities().IsDefinedHard(canRetrieve: true, canInsert: true, canUpdate: true, canDelete: false);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void InitializeProperties()
        {
            this.Name = "Operator";
            this.Description = "Operator";
            this.ClaimValue = MusicDBClaims.AclOperator;
        }

        #endregion
    }
}