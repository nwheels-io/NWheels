using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI;
using NWheels.UI.Behaviors;
using NWheels.UI.Widgets;

namespace NWheels.Modules.Security.UI
{
    public interface IUserSignInFormUiWidgetBuilder : IFormUiWidgetBuilder<IUserSignInRequest, IUserSignInUiState>
    {
        IUserSignInFormUiWidgetBuilder SignUpBehaviorUseBuiltIn();
        IUiBehaviorSelector<IUserSignInRequest, IUserSignInUiState, Unbound.Input> SignUpBehavior();
        IUserSignInFormUiWidgetBuilder ForgotPasswordBehaviorUseBuiltIn();
        IUiBehaviorSelector<IUserSignInRequest, IUserSignInUiState, Unbound.Input> ForgotPasswordBehavior();
    }
}
