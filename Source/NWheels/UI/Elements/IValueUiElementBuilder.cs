using System;
using System.Linq.Expressions;

namespace NWheels.UI.Elements
{
    public interface IValueUiElementBuilder<TModel, TState, TFluent> : IVisualUiElementBuilder<TModel, TState, TFluent>
        where TFluent : IVisualUiElementBuilder<TModel, TState, TFluent>
    {
        TFluent BindValueTo(Expression<Func<IUiScope<TModel, TState>, object>> path);
    }
}