using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Elements;

namespace NWheels.UI.Widgets
{
    public interface ICheckboxUiWidgetBuilder<TModel, TState> : 
        IValueUiElementBuilder<TModel, TState, ICheckboxUiWidgetBuilder<TModel, TState>>
    {
        ICheckboxUiWidgetBuilder<TModel, TState> StyleSlider();
    }
}
