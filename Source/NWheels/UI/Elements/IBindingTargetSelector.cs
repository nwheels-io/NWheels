using System;
using System.Linq.Expressions;

namespace NWheels.UI.Elements
{
    public interface IBindingTargetSelector<TModel, TState, TFluent, TValue>
    {
        TFluent To(Expression<Func<IUiScope<TModel, TState>, TValue>> path);
    }
}