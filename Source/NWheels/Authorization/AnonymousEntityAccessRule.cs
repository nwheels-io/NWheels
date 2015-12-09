using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Authorization.Claims;

namespace NWheels.Authorization
{
    public abstract class AnonymousEntityAccessRule : EntityAccessRuleClaim, IEntityAccessRule
    {
        public const string AnonymousEntityAccessRuleClaimValue = "System.EntityAccess.Anonymous";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected AnonymousEntityAccessRule()
            : base(AnonymousEntityAccessRuleClaimValue)
        {
            this.Rule = this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IEntityAccessRule

        public abstract void BuildAccessControl(IEntityAccessControlBuilder access);

        #endregion
    }
}
