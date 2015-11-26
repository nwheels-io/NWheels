using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
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

        public void LimitToAccountsOfType<T>() where T : IUserAccountEntity
        {
            this.Crud.FilterByType<T>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlCommand ChangePassword { get; set; }
        public EntityMethodForm<IUserAccountEntity, IChangePasswordInput> ChangePasswordForm { get; set; }

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

            presenter.On(ChangePassword).PopupEntityMethodForm(
                ChangePasswordForm, 
                onExecute: (user, data, state, payload) => user.SetPassword(state.Input.Password));

            Crud.AddEntityCommands(ChangePassword);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ViewModelContract]
        public interface IChangePasswordInput
        {
            [PropertyContract.Semantic.Password, PropertyContract.Security.Sensitive]
            SecureString Password { get; set; }
        }
    }
}
