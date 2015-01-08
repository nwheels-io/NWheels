using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Widgets
{
    public interface ITabbedContentUiWidgetBuilder<TModel, TState> : IBoundUiElementBuilder<TModel, TState>
    {
        ITabbedContentUiWidgetBuilder<TModel, TState> StyleAccordion();
        IUiLayoutBuilder<TModel, TState> AddTab(string title);
        ITabbedContentUiWidgetBuilder<TModel, TState> AddTab(string title, Action<IUiLayoutBuilder<TModel, TState>> contents);
    }
}
