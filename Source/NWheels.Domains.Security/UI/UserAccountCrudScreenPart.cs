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
    public class UserAccountCrudScreenPart<TDerivedEntity> : CrudScreenPart<TDerivedEntity>
        where TDerivedEntity : class, IUserAccountEntity
    {
        public UserAccountCrudScreenPart(string idName, UidlApplication parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void LimitToAccountsOfType<T>() where T : TDerivedEntity
        {
            this.Crud.FilterByType<T>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlCommand ChangePassword { get; set; }
        public EntityMethodForm<TDerivedEntity, IChangePasswordInput> ChangePasswordForm { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of CrudScreenPart<IUserAccountEntity>

        protected override void DescribePresenter(PresenterBuilder<CrudScreenPart<TDerivedEntity>, Empty.Data, IState> presenter)
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

            ChangePassword.Severity = CommandSeverity.Change;
            ChangePassword.Icon = "lock";

            ChangePasswordForm.InputForm.Field(x => x.NewPassword, modifiers: FormFieldModifiers.Password | FormFieldModifiers.Confirm);

            presenter.On(Crud.SelectedEntityChanged).Broadcast(ChangePasswordForm.EntitySetter).WithPayload(vm => vm.Input).TunnelDown();
            ChangePasswordForm.AttachTo(
                presenter,
                command: ChangePassword,
                onExecute: (user, vm) => user.SetPassword(vm.State.Input.NewPassword));

            Crud.AddEntityCommands(ChangePassword);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ViewModelContract]
        public interface IChangePasswordInput
        {
            [PropertyContract.Semantic.Password, PropertyContract.Security.Sensitive]
            SecureString NewPassword { get; set; }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class UserAccountCrudScreenPart : UserAccountCrudScreenPart<IUserAccountEntity>
    {
        public UserAccountCrudScreenPart(string idName, UidlApplication parent)
            : base(idName, parent)
        {
        }
    }
}
