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
        public const string ClaimValueString = "System.AllowAll";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of EntityAccessRuleEntity

        public override void BuildAccessControl(IEntityAccessControlBuilder builder)
        {
            builder.ToEntity<object>().IsDefinedHard(canRetrieve: true, canInsert: true, canUpdate: true, canDelete: true);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [EntityImplementation.TriggerOnNew]
        protected void EntityTriggerAfterNew()
        {
            this.Name = "Allow All";
            this.ClaimValue = ClaimValueString;
        }
    }
}
