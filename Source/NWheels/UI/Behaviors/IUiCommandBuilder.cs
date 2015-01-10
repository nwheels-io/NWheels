using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Behaviors
{
    public interface IUiCommandBuilder
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IUiCommandBuilder<TModel, TState> : IUiCommandBuilder, IBoundUiElementBuilder<TModel, TState>
    {
        IUiCommandBuilder<TModel, TState> EnableBy(Expression<Func<IUiScope<TModel, TState>, object>> path);
        IUiCommandBuilder<TModel, TState> DisableBy(Expression<Func<IUiScope<TModel, TState>, object>> path);
        IUiCommandBuilder<TModel, TState> HideWhileDisabled();
        IUiCommandBuilder<TModel, TState> Text(string displayText);
        IUiCommandBuilder<TModel, TState> Image(string imagePath);
        IUiCommandBuilder<TModel, TState> Behavior(Action<IUiBehaviorSelector<TModel, TState, Unbound.Input>> definition);
        IUiCommandBuilder<TModel, TState> BehaviorWithArgument<TArgument>(Action<IUiBehaviorSelector<TModel, TState, TArgument>> definition);
    }
}
