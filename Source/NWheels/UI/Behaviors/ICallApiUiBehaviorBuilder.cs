using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Elements;

namespace NWheels.UI.Behaviors
{
    public interface ICallUiBehaviorBuilder<TModel, TState, TInput> : 
        IUiElementBuilder<TModel, TState, ICallUiBehaviorBuilder<TModel, TState, TInput>>
    {
        ICallApiUiBehaviorBuilder<TModel, TState, TInput, TApi> Api<TApi>();
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface ICallApiUiBehaviorBuilder<TModel, TState, TInput, TApi> : 
        IUiElementBuilder<TModel, TState, ICallApiUiBehaviorBuilder<TModel, TState, TInput, TApi>>
    {
        IPromiseUiBehaviorBuilder<TModel, TState, TOutput> Method<TOutput>(Expression<Func<TApi, TOutput>> apiCall);
        IPromiseUiBehaviorBuilder<TModel, TState, TOutput> Method<TOutput>(Expression<Func<TModel, TApi, TOutput>> apiCall);
        IPromiseUiBehaviorBuilder<TModel, TState, TOutput> Method<TOutput>(Expression<Func<TModel, TState, TApi, TOutput>> apiCall);
        IPromiseUiBehaviorBuilder<TModel, TState, TOutput> Method<TOutput>(Expression<Func<TModel, TState, TInput, TApi, TOutput>> apiCall);
    }
}
