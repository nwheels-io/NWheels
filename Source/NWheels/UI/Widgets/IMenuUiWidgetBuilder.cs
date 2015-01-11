using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Behaviors;
using NWheels.UI.Elements;

namespace NWheels.UI.Widgets
{
    public interface IMenuUiWidgetBuilder<TModel, TState> : IVisualUiElementBuilder<TModel, TState, IMenuUiWidgetBuilder<TModel, TState>>
    {
        IMenuUiWidgetBuilder<TModel, TState> Item(
            Action<IMenuItemUiWidgetBuilder<TModel, TState>> definition);
        IMenuUiWidgetBuilder<TModel, TState> ItemBoundRange<TItemModel>(
            Expression<Func<IUiScope<TModel, TState>, IEnumerable<TItemModel>>> path,
            Action<IMenuItemUiWidgetBuilder<TItemModel, TState>> definition);
        IMenuUiWidgetBuilder<TModel, TState> ItemSeparator();
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IMenuItemUiWidgetBuilder<TModel, TState> : IVisualUiElementBuilder<TModel, TState, IMenuItemUiWidgetBuilder<TModel, TState>>
    {
        IUiCommandBuilder<TModel, TState> NewCommand();
        IMenuItemUiWidgetBuilder<TModel, TState> Command(IUiCommandBuilder commandToBindTo);
        IMenuItemUiWidgetBuilder<TModel, TState> CommandArgument(string name, object value);
        IMenuItemUiWidgetBuilder<TModel, TState> CommandArgument(string name, Expression<Func<IUiScope<TModel, TState>, object>> path);
        IMenuItemUiWidgetBuilder<TModel, TState> Text(string text);
        IMenuItemUiWidgetBuilder<TModel, TState> Text(Expression<Func<IUiScope<TModel, TState>, string>> path);
        IMenuItemUiWidgetBuilder<TModel, TState> Text(string format, params Expression<Func<IUiScope<TModel, TState>, object>>[] argumentPaths);
        IMenuItemUiWidgetBuilder<TModel, TState> SubMenu(Action<IMenuUiWidgetBuilder<TModel, TState>> definition);
    }
}
