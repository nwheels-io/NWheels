using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Behaviors;

namespace NWheels.UI
{
    public interface IBoundUiElementBuilder<TModel, TState> : IUiElementBuilder
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IBoundValueUiElementBuilder<TModel, TState, TFluent> : IBoundUiElementBuilder<TModel, TState>
        where TFluent : IBoundUiElementBuilder<TModel, TState>
    {
        TFluent BindValueTo(Expression<Func<IUiScope<TModel, TState>, object>> path);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IBoundUiElementBuilder<TModel, TState, TFluent> : IBoundUiElementBuilder<TModel, TState>
        where TFluent : IBoundUiElementBuilder<TModel, TState>
    {
        TFluent Initialize<T>(Expression<Func<IUiScope<TModel, TState>, T>> path, T value);
        IBindingTargetSelector<TModel, TState, TFluent, T> Bind<T>(Expression<Func<IUiScope<TModel, TState>, T>> path);
        IUiCommandBuilder<TModel, TState> Command();
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IBindingTargetSelector<TModel, TState, TFluent, TValue>
    {
        TFluent To(Expression<Func<IUiScope<TModel, TState>, TValue>> path);
    }
}
