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
    public interface IDockUiContainerLayout<TModel, TState> : IUiContainerBuilder<TModel, TState, IDockUiContainerLayout<TModel, TState>>
    {
        IUiWidgetSelector<TModel, TState> DockTop();
        IUiWidgetSelector<TModel, TState> DockRight();
        IUiWidgetSelector<TModel, TState> DockBottom();
        IUiWidgetSelector<TModel, TState> DockLeft();
        IUiWidgetSelector<TModel, TState> DockFill();
        IDockUiContainerLayout<TNewModel, TState> BindToModel<TNewModel>(Expression<Func<TModel, TNewModel>> path = null);
        IDockUiContainerLayout<TModel, TNewState> BindToUiState<TNewState>(Expression<Func<TState, TNewState>> path = null);
    }
}
