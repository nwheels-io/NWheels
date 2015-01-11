using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Elements;

namespace NWheels.UI.Behaviors
{
    public interface IUiCommandBuilder
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IUiCommandBuilder<TModel, TState> : IUiCommandBuilder, IUiElementBuilder
    {
        IUiCommandBuilder<TModel, TState> EnableIf(Expression<Func<IUiScope<TModel, TState>, bool>> condition);
        IUiCommandBuilder<TModel, TState> DisableIf(Expression<Func<IUiScope<TModel, TState>, bool>> condition);
        IUiCommandBuilder<TModel, TState> HideWhileDisabled();
        IUiCommandBuilder<TModel, TState> Text(string displayText);
        IUiCommandBuilder<TModel, TState> Text(Expression<Func<IUiScope<TModel, TState>, string>> binding);
        IUiCommandBuilder<TModel, TState> Text(string format, params Expression<Func<IUiScope<TModel, TState>, object>>[] formatArgBindings);
        IUiCommandBuilder<TModel, TState> Icon(string imagePath);
        IUiCommandBuilder<TModel, TState> Icon(Expression<Func<IUiScope<TModel, TState>, string>> binding);
        IUiCommandBuilder<TModel, TState> Icon(string format, params Expression<Func<IUiScope<TModel, TState>, object>>[] formatArgBindings);
        IUiCommandBuilder<TModel, TState> Behavior(Action<IUiBehaviorSelector<TModel, TState, Unbound.Input>> definition);
        IUiCommandBuilder<TModel, TState> BehaviorWithArgument<TArgument>(Action<IUiBehaviorSelector<TModel, TState, TArgument>> definition);
    }
}
