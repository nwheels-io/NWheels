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
        TFluent BindValueToModel(Expression<Func<TModel, object>> path);
        TFluent BindValueToUiState(Expression<Func<TState, object>> path);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IBoundUiElementBuilder<TModel, TState, TFluent> : IBoundUiElementBuilder<TModel, TState>
        where TFluent : IBoundUiElementBuilder<TModel, TState>
    {
        TFluent InitializeModel<T>(Expression<Func<TModel, T>> path, T value);
        TFluent InitializeUiState<T>(Expression<Func<TState, T>> path, T value);
        IBindingTargetSelector<TModel, TState, TFluent, T> BindModel<T>(Expression<Func<TModel, T>> path);
        IBindingTargetSelector<TModel, TState, TFluent, T> BindUiState<T>(Expression<Func<TState, T>> path);
        IUiCommandBuilder<TModel, TState> Command();
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IBindingTargetSelector<TModel, TState, TFluent, TValue>
    {
        TFluent ToModel(Expression<Func<TModel, TValue>> path);
        TFluent ToUiState(Expression<Func<TState, TValue>> path);
    }
}
