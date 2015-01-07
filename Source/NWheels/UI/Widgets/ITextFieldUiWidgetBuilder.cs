using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Widgets
{
    public interface ITextFieldUiWidgetBuilder<TModel, TState> : IBoundValueUiElementBuilder<TModel, TState, ITextFieldUiWidgetBuilder<TModel, TState>>
    {
        IAutocompleteBindingTargetSelector<TModel, TState, TResult> AutocompleteApi<TApi, TResult>(
            Expression<Func<TApi, Func<string, IEnumerable<TResult>>>> apiMethod);

        IAutocompleteBindingTargetSelector<TModel, TState, TResult> AutocompleteApi<TApi, TResult>(
            Expression<Func<TModel, TApi, Func<TModel, IEnumerable<TResult>>>> apiMethod);
        
        IAutocompleteBindingTargetSelector<TModel, TState, TResult> AutocompleteApi<TApi, TResult>(
            Expression<Func<TState, TApi, Func<TModel, IEnumerable<TResult>>>> apiMethod);
     
        IAutocompleteBindingTargetSelector<TModel, TState, TResult> AutocompleteApi<TApi, TResult>(
            Expression<Func<TModel, TState, TApi, Func<TModel, TState, IEnumerable<TResult>>>> apiMethod);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IAutocompleteBindingTargetSelector<TModel, TState, TResult>
    {
        ITextFieldUiWidgetBuilder<TModel, TState> BindToModel(Expression<Func<TModel, TResult>> path);
        ITextFieldUiWidgetBuilder<TModel, TState> BindToUiState(Expression<Func<TState, TResult>> path);
    }

}
