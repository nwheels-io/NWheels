using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI;
using NWheels.UI.Toolbox;
using NWheels.UI.Uidl;

namespace NWheels.Domains.Security.UI
{
    public class UserAccountCrudScreenPart : CrudScreenPart<IUserAccountEntity>
    {
        public UserAccountCrudScreenPart(string idName, UidlApplication parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of CrudScreenPart<IUserAccountEntity>

        protected override void DescribePresenter(PresenterBuilder<CrudScreenPart<IUserAccountEntity>, Empty.Data, Empty.State> presenter)
        {
            base.DescribePresenter(presenter);
            this.GridColumns(x => x.LoginName, x => x.FullName, x => x.AssociatedRoles, x => x.LastLoginAtUtc, x => x.IsLockedOut);

            if ( Crud.Form != null )
            {
                base.Crud.Form.HideFields(x => x.Passwords);
            }

            if ( Crud.FormTypeSelector != null )
            {
                foreach ( var form in Crud.FormTypeSelector.WidgetsOfType<IUidlForm>() )
                {
                    form.HideFields("Passwords");
                }
            }
        }

        #endregion
    }
}
