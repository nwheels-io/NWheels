using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Layouts
{
    public interface IFlowUiContainerLayout<TModel, TState> : IBoundUiElementBuilder<TModel, TState, IFlowUiContainerLayout<TModel, TState>>
    {
        IUiContainerBuilder<TModel, TState> Contents();
        IFlowUiContainerLayout<TNewModel, TState> BindToModel<TNewModel>(Expression<Func<TModel, TNewModel>> path = null);
        IFlowUiContainerLayout<TModel, TNewState> BindToUiState<TNewState>(Expression<Func<TState, TNewState>> path = null);
    }
}
