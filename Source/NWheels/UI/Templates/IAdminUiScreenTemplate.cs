using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Layouts;

namespace NWheels.UI.Templates
{
    public interface IAdminUiScreenTemplate<TModel, TState> : IBoundUiElementBuilder<TModel, TState, IAdminUiScreenTemplate<TModel, TState>>
    {
        IAdminUiScreenTemplate<TNewModel, TState> BindToModel<TNewModel>(Expression<Func<TModel, TNewModel>> path = null);
        IAdminUiScreenTemplate<TModel, TNewState> BindToUiState<TNewState>(Expression<Func<TState, TNewState>> path = null);
    }
}
