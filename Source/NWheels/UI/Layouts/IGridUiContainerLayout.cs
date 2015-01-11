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
    public interface IGridUiContainerLayout<TModel, TState> : IUiContainerBuilder<TModel, TState, IGridUiContainerLayout<TModel, TState>>
    {
        IGridRowBuilder<TModel, TState> NewRow();
        IGridRowBuilder<TModel, TState> Row();
        IGridRowBuilder<TModel, TState> Row(int index);
        IGridUiContainerLayout<TNewModel, TState> BindToModel<TNewModel>(Expression<Func<TModel, TNewModel>> path = null);
        IGridUiContainerLayout<TModel, TNewState> BindToUiState<TNewState>(Expression<Func<TState, TNewState>> path = null);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IGridRowBuilder<TModel, TState> : IVisualUiElementBuilder<TModel, TState, IGridRowBuilder<TModel, TState>>
    {
        IGridColumnBuilder<TModel, TState> NewCol();
        IGridColumnBuilder<TModel, TState> Col();
        IGridColumnBuilder<TModel, TState> Col(int index);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IGridColumnBuilder<TModel, TState> : IUiWidgetSelector<TModel, TState>
    {
        IGridColumnBuilder<TModel, TState> ColSpan(int columns);
    }
}
