using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Elements;
using NWheels.UI.Layouts;
using NWheels.UI;

namespace NWheels.Modules.Security.UI
{
    public interface ISignInUiScreenTemplate<TModel, TState> : IUiContainerBuilder<TModel, TState, ISignInUiScreenTemplate<TModel, TState>>
    {
        IUserSignInFormUiWidgetBuilder Form();
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public static class AuthModuleScreenTemplates
    {
        public static ISignInUiScreenTemplate<TModel, TState> TemplateSignIn<TModel, TState>(
            this IUiScreenBuilder<TModel, TState> selector)
        {
            return selector.CreateChildBuilder<ISignInUiScreenTemplate<TModel, TState>>();
        }
    }
}
