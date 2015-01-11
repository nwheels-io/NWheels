using System;
using System.Linq.Expressions;

namespace NWheels.UI.Elements
{
    public interface IVisualUiElementBuilder<TModel, TState, TFluent> : IUiElementBuilder
        where TFluent : IVisualUiElementBuilder<TModel, TState, TFluent>
    {
        TFluent ShowIf(Expression<Func<IUiScope<TModel, TState>, bool>> condition);
        TFluent HideIf(Expression<Func<IUiScope<TModel, TState>, bool>> condition);
        TFluent EnableIf(Expression<Func<IUiScope<TModel, TState>, bool>> condition);
        TFluent DisableIf(Expression<Func<IUiScope<TModel, TState>, bool>> condition);
        TFluent HiddenWhileDisabled();
    }
}