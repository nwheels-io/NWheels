using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Behaviors
{
    public interface IUiBehaviorSelector<TModel, TState> : IBoundUiElementBuilder<TModel, TState>
    {
        IPromiseUiBehaviorBuilder<TModel, TState, TResult> InvokeApi<TApi, TResult>(Expression<Func<TModel, TState, TApi, TResult>> apiCall);

        IMoveUiBehaviorBuilder<TModel, TState> Move();
    }
}
