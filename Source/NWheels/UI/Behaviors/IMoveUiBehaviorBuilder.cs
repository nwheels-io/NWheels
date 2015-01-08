using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace NWheels.UI.Behaviors
{
    public interface IMoveUiBehaviorBuilder<TModel, TState> : IBoundUiElementBuilder<TModel, TState>
    {
        IMoveBehaviorTargetSelector<TModel, TState, TData> FromModel<TData>(Expression<Func<TModel, TData>> path);
        IMoveBehaviorTargetSelector<TModel, TState, TData> FromUiState<TData>(Expression<Func<TState, TData>> path);
        IMoveBehaviorTargetSelector<TModel, TState, TResult> FromApiResult<TResult>();
        IMoveBehaviorTargetSelector<TModel, TState, TData> FromApiResult<TResult, TData>(Expression<Func<TResult, TData>> path);
        IMoveBehaviorTargetSelector<TModel, TState, TData> FromApiError<TData>(Expression<Func<IApiErrorState, TData>> path);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IMoveBehaviorTargetSelector<TModel, TState, TData> : IBoundUiElementBuilder<TModel, TState>
    {
        IPromiseUiBehaviorBuilder<TModel, TState, TData> ToModel(Expression<Func<TModel, TData>> path);
        IPromiseUiBehaviorBuilder<TModel, TState, TData> ItemAddToModel(Expression<Func<TModel, IList<TData>>> path);
        IPromiseUiBehaviorBuilder<TModel, TState, TData> ItemsAddRangeToModel(Expression<Func<TModel, TData>> path);
        IPromiseUiBehaviorBuilder<TModel, TState, TData> ToUiState(Expression<Func<TState, TData>> path);
        IPromiseUiBehaviorBuilder<TModel, TState, TData> ItemAddToUiState(Expression<Func<TState, IList<TData>>> path);
        IPromiseUiBehaviorBuilder<TModel, TState, TData> ItemsAddRangeToUiState(Expression<Func<TState, TData>> path);
    }
}
