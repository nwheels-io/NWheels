using System;
using System.Linq.Expressions;
using NWheels.UI.Templates;

namespace NWheels.UI.Elements
{
    public interface IUiScreenBuilder<TModel, TState> : IUiContainerBuilder<TModel, TState, IUiScreenBuilder<TModel, TState>>
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IFrontUiScreenTemplate<TModel, TState> TemplateFront<TModel, TState>(
            this IUiScreenBuilder<TModel, TState> selector)
        {
            return selector.CreateChildBuilder<IFrontUiScreenTemplate<TModel, TState>>();
        }
    }
}
