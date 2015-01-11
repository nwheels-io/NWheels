using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Elements;
using NWheels.UI.Layouts;

namespace NWheels.UI.Widgets
{
    public interface ITabbedContentUiWidgetBuilder<TModel, TState> : IVisualUiElementBuilder<TModel, TState, ITabbedContentUiWidgetBuilder<TModel, TState>>
    {
        ITabbedContentUiWidgetBuilder<TModel, TState> StyleAccordion();
        IUiLayoutBuilder<TModel, TState> AddTab(string title);
        ITabbedContentUiWidgetBuilder<TModel, TState> AddTab(string title, Action<IUiLayoutBuilder<TModel, TState>> contents);
    }
}
