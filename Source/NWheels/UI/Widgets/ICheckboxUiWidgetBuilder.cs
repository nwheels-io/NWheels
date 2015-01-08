using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Widgets
{
    public interface ICheckboxUiWidgetBuilder<TModel, TState> : 
        IBoundValueUiElementBuilder<TModel, TState, ICheckboxUiWidgetBuilder<TModel, TState>>
    {
        ICheckboxUiWidgetBuilder<TModel, TState> StyleSlider();
    }
}
