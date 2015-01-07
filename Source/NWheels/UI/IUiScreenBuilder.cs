using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Autofac;
using NWheels.UI.Templates;

namespace NWheels.UI
{
    public interface IUiScreenBuilder<TModel, TState> : IBoundUiElementBuilder<TModel, TState, IUiScreenBuilder<TModel, TState>>
    {
        IUiScreenBuilder<TModel, TState> SetAsHome();
        IUiScreenBuilder<TNewModel, TState> BindToModel<TNewModel>(Expression<Func<TModel, TNewModel>> path = null);
        IUiScreenBuilder<TModel, TNewState> BindToUiState<TNewState>(Expression<Func<TState, TNewState>> path = null);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public static class UiScreenBuilderTemplateChoices
    {
        public static IWizardUiScreenTemplate<TModel, TState> TemplateWizard<TModel, TState>(
            this IUiScreenBuilder<TModel, TState> selector)
        {
            return selector.CreateChildBuilder<IWizardUiScreenTemplate<TModel, TState>>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IAdminUiScreenTemplate<TModel, TState> TemplateAdmin<TModel, TState>(
            this IUiScreenBuilder<TModel, TState> selector)
        {
            return selector.CreateChildBuilder<IAdminUiScreenTemplate<TModel, TState>>();
        }
    }
}
