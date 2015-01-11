using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Elements;

namespace NWheels.UI.Widgets
{
    public interface IImageUiWidgetBuilder<TModel, TState> : IVisualUiElementBuilder<TModel, TState, IImageUiWidgetBuilder<TModel, TState>>
    {
    }
}
