using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace NWheels.UI.Behaviors
{
    public interface IMoveUiBehaviorBuilder<TModel, TState, TInput> : IBoundUiElementBuilder<TModel, TState>
    {
        IMoveBehaviorTargetSelector<TModel, TState, TData> From<TData>(Expression<Func<IUiScope<TModel, TState, TInput>, TData>> path);
        IMoveBehaviorTargetSelector<TModel, TState, TData> FromApiError<TData>(Expression<Func<IApiErrorState, TData>> path);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IMoveBehaviorTargetSelector<TModel, TState, TData> : IBoundUiElementBuilder<TModel, TState>
    {
        IPromiseUiBehaviorBuilder<TModel, TState, TData> To(Expression<Func<IUiScope<TModel, TState>, TData>> path);
        IPromiseUiBehaviorBuilder<TModel, TState, TData> AddItemTo(Expression<Func<IUiScope<TModel, TState>, IList<TData>>> path);
        IPromiseUiBehaviorBuilder<TModel, TState, TData> AddMultipleItemsTo(Expression<Func<IUiScope<TModel, TState>, TData>> path);
    }
}
