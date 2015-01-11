using System;
using System.Linq.Expressions;
using NWheels.UI.Elements;

namespace NWheels.UI.Layouts
{
    public interface IUiLayoutBuilder<TModel, TState> : IUiContainerBuilder<TModel, TState, IUiLayoutBuilder<TModel, TState>>
    {
        IUiLayoutBuilder<TNewModel, TState> BindToModel<TNewModel>(Expression<Func<TModel, TNewModel>> path = null);
        IUiLayoutBuilder<TModel, TNewState> BindToUiState<TNewState>(Expression<Func<TState, TNewState>> path = null);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public static class UiLayoutChoices
    {
        public static IDockUiContainerLayout<TModel, TState> LayoutDock<TModel, TState>(
            this IUiLayoutBuilder<TModel, TState> container)
        {
            return container.CreateChildBuilder<IDockUiContainerLayout<TModel, TState>>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IStackUiContainerLayout<TModel, TState> LayoutStack<TModel, TState>(
            this IUiLayoutBuilder<TModel, TState> container)
        {
            return container.CreateChildBuilder<IStackUiContainerLayout<TModel, TState>>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IGridUiContainerLayout<TModel, TState> LayoutGrid<TModel, TState>(
            this IUiLayoutBuilder<TModel, TState> container)
        {
            return container.CreateChildBuilder<IGridUiContainerLayout<TModel, TState>>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IFlowUiContainerLayout<TModel, TState> LayoutFlow<TModel, TState>(
            this IUiLayoutBuilder<TModel, TState> container)
        {
            return container.CreateChildBuilder<IFlowUiContainerLayout<TModel, TState>>();
        }
    }
}
