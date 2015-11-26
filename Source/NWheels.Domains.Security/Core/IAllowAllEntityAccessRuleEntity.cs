using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Authorization;
using NWheels.Entities;

namespace NWheels.Domains.Security.Core
{
    [EntityContract]
    public interface IAllowAllEntityAccessRuleEntity : IEntityAccessRuleEntity
    {
    }
    public abstract class AllowAllEntityAccessRuleEntity : EntityAccessRuleEntity, IAllowAllEntityAccessRuleEntity
    {
        public const string ClaimValueString = "System.EntityRule.AllowAll";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void BuildAccessControl(IEntityAccessControlBuilder builder)
        {
            builder.ToAllEntities().IsDefinedHard(canRetrieve: true, canInsert: true, canUpdate: true, canDelete: true);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void InitializeProperties()
        {
            this.Name = "Allow All";
            this.ClaimValue = ClaimValueString;
        }
    }
}
