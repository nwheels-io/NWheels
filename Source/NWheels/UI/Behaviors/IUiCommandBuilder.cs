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
        IUiCommandBuilder<TModel, TState> EnableByModel(Expression<Func<TModel, object>> path);
        IUiCommandBuilder<TModel, TState> DisableByModel(Expression<Func<TModel, object>> path);
        IUiCommandBuilder<TModel, TState> EnableByUiState(Expression<Func<TState, object>> path);
        IUiCommandBuilder<TModel, TState> DisableByUiState(Expression<Func<TModel, object>> path);
        IUiCommandBuilder<TModel, TState> Text(string displayText);
        IUiCommandBuilder<TModel, TState> Image(string imagePath);
        IUiBehaviorSelector<TModel, TState> Behavior();
    }
}
