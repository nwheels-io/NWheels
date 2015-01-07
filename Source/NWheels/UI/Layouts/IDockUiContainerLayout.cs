using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Layouts
{
    public interface IDockUiContainerLayout<TModel, TState> : IBoundUiElementBuilder<TModel, TState, IDockUiContainerLayout<TModel, TState>>
    {
        IUiContainerBuilder<TModel, TState> DockTop();
        IUiContainerBuilder<TModel, TState> DockRight();
        IUiContainerBuilder<TModel, TState> DockBottom();
        IUiContainerBuilder<TModel, TState> DockLeft();
        IUiContainerBuilder<TModel, TState> DockFill();
        IDockUiContainerLayout<TNewModel, TState> BindToModel<TNewModel>(Expression<Func<TModel, TNewModel>> path = null);
        IDockUiContainerLayout<TModel, TNewState> BindToUiState<TNewState>(Expression<Func<TState, TNewState>> path = null);
    }
}
