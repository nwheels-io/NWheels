using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Behaviors;

namespace NWheels.UI.Widgets
{
    public interface IMenuUiWidgetBuilder<TModel, TState> : IBoundUiElementBuilder<TModel, TState>
    {
        IMenuUiWidgetBuilder<TModel, TState> Item(
            Action<IMenuItemUiWidgetBuilder<TModel, TState>> definition);
        IMenuUiWidgetBuilder<TItem, TState> ItemFromModel<TItem>(
            Expression<Func<TModel, TItem>> path,
            Action<IMenuItemUiWidgetBuilder<TModel, TState>> definition);
        IMenuUiWidgetBuilder<TItem, TState> ItemFromUiState<TItem>(
            Expression<Func<TState, TItem>> path,
            Action<IMenuItemUiWidgetBuilder<TModel, TState>> definition);
        IMenuUiWidgetBuilder<TItem, TState> MultipleItemsFromModel<TItem>(
            Expression<Func<TModel, IEnumerable<TItem>>> pathToItems,
            Action<IMenuItemUiWidgetBuilder<TModel, TState>> definition);
        IMenuUiWidgetBuilder<TItem, TState> MultipleItemsFromUiState<TItem>(
            Expression<Func<TState, IEnumerable<TItem>>> pathToItems,
            Action<IMenuItemUiWidgetBuilder<TModel, TState>> definition);
        IMenuUiWidgetBuilder<TModel, TState> Separator();
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IMenuItemUiWidgetBuilder<TModel, TState> : IBoundUiElementBuilder<TModel, TState>
    {
        IUiCommandBuilder<TModel, TState> NewCommand();
        IMenuItemUiWidgetBuilder<TModel, TState> Command(IUiCommandBuilder commandToBindTo);
        IMenuItemUiWidgetBuilder<TModel, TState> CommandArgument(string name, object value);
        IMenuItemUiWidgetBuilder<TModel, TState> CommandArgument(string name, Expression<Func<IUiScope<TModel, TState>, object>> path);
        IMenuItemUiWidgetBuilder<TModel, TState> Text(string text);
        IMenuItemUiWidgetBuilder<TModel, TState> Text(Expression<Func<IUiScope<TModel, TState>, string>> path);
        IMenuItemUiWidgetBuilder<TModel, TState> Text(string format, params Expression<Func<IUiScope<TModel, TState>, object>>[] argumentPaths);
        IMenuItemUiWidgetBuilder<TModel, TState> HiddenWhileDisabled();
        IMenuUiWidgetBuilder<TModel, TState> SubMenu();
    }
}
