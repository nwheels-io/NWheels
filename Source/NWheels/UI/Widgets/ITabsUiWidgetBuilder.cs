using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Widgets
{
    public interface ITabsUiWidgetBuilder<TModel, TState> : IBoundUiElementBuilder<TModel, TState>
    {
        IUiLayoutBuilder<TModel, TState> AddTab(string title);
        ITabsUiWidgetBuilder<TModel, TState> AddTab(string title, Action<IUiLayoutBuilder<TModel, TState>> contents);
    }
}
