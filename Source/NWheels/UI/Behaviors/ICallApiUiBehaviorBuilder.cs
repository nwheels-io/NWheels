using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Behaviors
{
    public interface ICallUiBehaviorBuilder<TModel, TState, TInput> : IBoundUiElementBuilder<TModel, TState>
    {
        ICallApiUiBehaviorBuilder<TModel, TState, TInput, TApi> Api<TApi>();
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface ICallApiUiBehaviorBuilder<TModel, TState, TInput, TApi> : IBoundUiElementBuilder<TModel, TState>
    {
        IPromiseUiBehaviorBuilder<TModel, TState, TOutput> Method<TOutput>(Expression<Func<TApi, TOutput>> apiCall);
        IPromiseUiBehaviorBuilder<TModel, TState, TOutput> Method<TOutput>(Expression<Func<TModel, TApi, TOutput>> apiCall);
        IPromiseUiBehaviorBuilder<TModel, TState, TOutput> Method<TOutput>(Expression<Func<TModel, TState, TApi, TOutput>> apiCall);
        IPromiseUiBehaviorBuilder<TModel, TState, TOutput> Method<TOutput>(Expression<Func<TModel, TState, TInput, TApi, TOutput>> apiCall);
    }
}
