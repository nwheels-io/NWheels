using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Elements;
using NWheels.UI.Widgets;

namespace NWheels.UI.Templates
{
    public interface IFrontUiScreenTemplate<TModel, TState> : IUiContainerBuilder<TModel, TState, IFrontUiScreenTemplate<TModel, TState>>
    {
        ILogoUiWidgetBuilder<TModel, TState> Logo();



        IFrontUiScreenTemplate<TNewModel, TState> BindToModel<TNewModel>(Expression<Func<TModel, TNewModel>> path = null);
        IFrontUiScreenTemplate<TModel, TNewState> BindToUiState<TNewState>(Expression<Func<TState, TNewState>> path = null);
    }
}
