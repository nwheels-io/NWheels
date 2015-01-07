using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NWheels.Modules.Auth;

namespace NWheels.UI.Widgets
{
    public interface ILoggedInUserUiWidgetBuilder<TModel, TState> : IBoundUiElementBuilder<TModel, TState>
    {
        ILoggedInUserUiWidgetBuilder<TModel, TState> DisplayFormat(string format, params Expression<Func<IUserAccountEntity, object>>[] values);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        ILoggedInUserUiWidgetBuilder<TModel, TState> LogoutCommand();
    }
}
