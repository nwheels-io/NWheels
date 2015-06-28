using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
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
                .CallApi<ISecurityDomainApi>().RequestReply((api, data, state, input) => api.LogUserIn(data))
                .Then(
                    onSuccess: b => b.Broadcast(UserLoggedIn).BubbleUp(),
                    onFailure: b => b.UserAlertFrom<IAlerts>().ShowInline((r, d, s, failure) => r.LoginHasFailed(failure.ReasonText)));

        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlCommand LogIn { get; set; }
        public UidlCommand SignUp { get; set; }
        public UidlCommand ForgotPassword { get; set; }
        public UidlNotification UserLoggedIn { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IAlerts : IUserAlertRepository
        {
            [ErrorAlert(UserAlertResult.OK)]
            UidlUserAlert LoginHasFailed(string reason);
            
            [WarningAlert(UserAlertResult.OK)]
            UidlUserAlert UserWasNotLoggedOut(string reason);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        [ViewModelContract]
        public interface IState
        {
            [ViewModelPropertyContract.PersistedOnUserMachine]
            bool RememberMe { get; set; }
        }
    }
}
