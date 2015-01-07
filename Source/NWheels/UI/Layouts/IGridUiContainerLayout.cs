using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Layouts
{
    public interface IGridUiContainerLayout<TModel, TState> : IBoundUiElementBuilder<TModel, TState, IGridUiContainerLayout<TModel, TState>>
    {
        IGridRowBuilder<TModel, TState> Row(int? index = null);
        IGridUiContainerLayout<TNewModel, TState> BindToModel<TNewModel>(Expression<Func<TModel, TNewModel>> path = null);
        IGridUiContainerLayout<TModel, TNewState> BindToUiState<TNewState>(Expression<Func<TState, TNewState>> path = null);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IGridRowBuilder<TModel, TState> : IBoundUiElementBuilder<TModel, TState>
    {
        IGridColumnBuilder<TModel, TState> Column(int? index = null);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IGridColumnBuilder<TModel, TState> : IUiContainerBuilder<TModel, TState>
    {
        IGridColumnBuilder<TModel, TState> ColSpan(int columns);
    }
}
