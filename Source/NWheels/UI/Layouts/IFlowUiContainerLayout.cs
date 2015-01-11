using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Elements;
using NWheels.UI.Widgets;

namespace NWheels.UI.Layouts
{
    public interface IFlowUiContainerLayout<TModel, TState> : 
        IUiContainerBuilder<TModel, TState, IFlowUiContainerLayout<TModel, TState>>
    {
        IFlowUiContainerLayout<TNewModel, TState> BindToModel<TNewModel>(Expression<Func<TModel, TNewModel>> path = null);
        IFlowUiContainerLayout<TModel, TNewState> BindToUiState<TNewState>(Expression<Func<TState, TNewState>> path = null);
        IFlowUiContainerLayout<TModel, TState> AddLast(Action<IUiWidgetSelector<TModel, TState>> widgetDefinition);
    }
}
