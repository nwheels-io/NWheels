using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI;
using NWheels.UI.Uidl;

namespace NWheels.Domains.Security.UI
{
    public class CurrentLoggedInUser : WidgetBase<CurrentLoggedInUser, ILogUserOutRequest, CurrentLoggedInUser.IState>
    {
        public CurrentLoggedInUser(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<CurrentLoggedInUser, ILogUserOutRequest, IState> presenter)
        {
            presenter.On(LogOut)
                .InvokeTransactionScript<UserLogoutTransactionScript>().FireAndForget((logout, data, state, input) => logout.Execute())
                .Then(
                    onSuccess: b => b.Broadcast(UserLoggedOut).BubbleUp(),
                    onFailure: b => b.UserAlertFrom<IAlerts>().ShowInline((r, d, s, failure) => r.LogoutOperationFailed(failure.ReasonText))
                        .Then(bb => bb.Broadcast(UserLoggedOut).BubbleUp()));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlCommand EditProfile { get; set; }
        public UidlCommand LogOut { get; set; }
        public UidlNotification UserLoggedOut { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ViewModelContract]
        public interface IState
        {
            
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IAlerts : IUserAlertRepository
        {
            [ErrorAlert(UserAlertResult.OK)]
            UidlUserAlert LogoutOperationFailed(string reason);
        }
    }
}
