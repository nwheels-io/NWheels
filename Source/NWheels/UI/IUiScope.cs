using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    public interface IUiScope<TModel, TState>
    {
        bool IsValid(Expression<Func<IUiScope<TModel, TState>, bool>> path);
        bool IsModified(Expression<Func<IUiScope<TModel, TState>, bool>> path);
        bool IsTouched(Expression<Func<IUiScope<TModel, TState>, bool>> path);
        TModel Model { get; }
        TState UiState { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IUiScope<TModel, TState, out TInput> : IUiScope<TModel, TState>
    {
        TInput Input { get; }
    }
}
