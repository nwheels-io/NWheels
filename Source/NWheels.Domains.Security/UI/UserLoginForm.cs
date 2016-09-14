#if false

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NWheels.Authorization.Impl;
using NWheels.DataObjects;
using NWheels.Domains.Security.Core;
using NWheels.Domains.Security.Impl;
using NWheels.Globalization;
using NWheels.Globalization.Core;
using NWheels.UI;
using NWheels.UI.Uidl;

namespace NWheels.Domains.Security.UI
{
    public class UserLoginForm : WidgetBase<UserLoginForm, ILogUserInRequest, UserLoginForm.IState>
    {
        public UserLoginForm(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerable<LocaleEntryKey> GetTranslatables()
        {
            return base.GetTranslatables().Concat(LocaleEntryKey.Enumerate(
                this,                
                "LoginName", null,
                "Password", null,
                "EnterLoginName", null,
                "EnterPassword", null,
                "SignUp", null,
                "ForgotPassword", null,
                "RememberMe", null
            ));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<UserLoginForm, ILogUserInRequest, IState> presenter)
        {
            presenter.On(LogIn)
                .InvokeTransactionScript<UserLoginTransactionScript>()
                .WaitForReply((x, vm) => x.Execute(vm.Data.LoginName, vm.Data.Password))
                .Then(
                    onSuccess: b => b.AlterModel(alt => alt.Copy(m => m.Input).To(m => m.State.User))
                        .Then(bb => bb.Broadcast(UserLoggedIn).WithPayload(m => m.Input).BubbleUp()),
                    onFailure: b => b.AlterModel(alt => alt.Copy(m => m.Input.FaultCode).To(m => m.State.LoginFault))
                        .Then(bb => bb.UserAlertFrom<IAlerts>().ShowInline(
                            (x, vm) => x.LoginHasFailed(vm.Input.FaultReason),
                            faultInfo: vm => vm.Input)));

            presenter.On(ChangePassword)
                .InvokeTransactionScript<ChangePasswordTransactionScript>()
                .WaitForCompletion((x, vm) => x.Execute(vm.Data.LoginName, vm.Data.Password, vm.Data.NewPassword))
                .Then(
                    onSuccess: b => b.AlterModel(alt => alt.Copy(m => (string)null).To(m => m.State.LoginFault))
                        .Then(bb => bb.UserAlertFrom<IAlerts>().ShowInline((x, vm) => x.PasswordSuccessfullyChanged())),
                    onFailure: b => b.AlterModel(alt => alt.Copy(m => m.Input.FaultCode).To(m => m.State.LoginFault))
                        .Then(bb => bb.UserAlertFrom<IAlerts>().ShowInline(
                            (x, vm) => x.FailedToChangePassword(vm.Input.FaultReason), 
                            faultInfo: vm => vm.Input)));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlCommand LogIn { get; set; }
        public UidlCommand SignUp { get; set; }
        public UidlCommand ForgotPassword { get; set; }
        public UidlCommand ChangePassword { get; set; }
        public UidlNotification<UserLoginTransactionScript.Result> UserLoggedIn { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IAlerts : IUserAlertRepository
        {
            [ErrorAlert]
            UidlUserAlert LoginHasFailed(string reason);
            
            [WarningAlert]
            UidlUserAlert UserWasNotLoggedOut(string reason);

            [InfoAlert]
            UidlUserAlert PasswordSuccessfullyChanged();

            [ErrorAlert]
            UidlUserAlert FailedToChangePassword(string reason);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        [ViewModelContract]
        public interface IState
        {
            string LoginFault { get; set; }
            
            UserLoginTransactionScript.Result User { get; set; }

            [ViewModelPropertyContract.PersistedOnUserMachine]
            bool RememberMe { get; set; }
        }
    }
}

#endif