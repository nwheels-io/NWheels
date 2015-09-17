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

        public override IEnumerable<string> GetTranslatables()
        {
            return base.GetTranslatables().Concat(new[] {
                "LoginName", "Password", "EnterLoginName", "EnterPassword", "SignUp", "ForgotPassword", "RememberMe"
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<UserLoginForm, ILogUserInRequest, IState> presenter)
        {
            presenter.On(LogIn)
                .InvokeTransactionScript<UserLoginTransactionScript>()
                .WaitForReply((x, data, state, input) => x.Execute(data.LoginName, data.Password))
                .Then(
                    onSuccess: b => b.AlterModel(alt => alt.Copy(m => m.Input).To(m => m.State.User))
                        .Then(bb => bb.Broadcast(UserLoggedIn).WithPayload(m => m.Input).BubbleUp()),
                    onFailure: b => b.AlterModel(alt => alt.Copy(m => m.Input.FaultCode).To(m => m.State.LoginFault))
                        .Then(bb => bb.UserAlertFrom<IAlerts>().ShowInline((r, d, s, failure) => r.LoginHasFailed(failure.FaultReason))));

            presenter.On(ChangePassword)
                .InvokeTransactionScript<ChangePasswordTransactionScript>()
                .WaitForCompletion((x, data, state, input) => x.Execute(data.LoginName, data.Password, data.NewPassword))
                .Then(
                    onSuccess: b => b.AlterModel(alt => alt.Copy(m => (string)null).To(m => m.State.LoginFault))
                        .Then(bb => bb.UserAlertFrom<IAlerts>().ShowInline((r, d, s, failure) => r.PasswordSuccessfullyChanged())),
                    onFailure: b => b.AlterModel(alt => alt.Copy(m => m.Input.FaultCode).To(m => m.State.LoginFault))
                        .Then(bb => bb.UserAlertFrom<IAlerts>().ShowInline((r, d, s, failure) => r.FailedToChangePassword(failure.FaultReason))));
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
            [ErrorAlert(UserAlertResult.OK)]
            UidlUserAlert LoginHasFailed(string reason);
            
            [WarningAlert(UserAlertResult.OK)]
            UidlUserAlert UserWasNotLoggedOut(string reason);

            [InfoAlert(UserAlertResult.OK)]
            UidlUserAlert PasswordSuccessfullyChanged();

            [ErrorAlert(UserAlertResult.OK)]
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
