using System;
using System.Linq.Expressions;
using NWheels.UI.Behaviors;

namespace NWheels.UI.Elements
{
    public interface IUiContainerBuilder<TModel, TState, TFluent> : IVisualUiElementBuilder<TModel, TState, TFluent>
        where TFluent : IVisualUiElementBuilder<TModel, TState, TFluent>
    {
        TFluent BorderStyleHighlight();
        TFluent Initialize<T>(Expression<Func<IUiScope<TModel, TState>, T>> path, T value);
        IBindingTargetSelector<TModel, TState, TFluent, T> Bind<T>(Expression<Func<IUiScope<TModel, TState>, T>> path);
        IUiCommandBuilder<TModel, TState> Command();
    }
}