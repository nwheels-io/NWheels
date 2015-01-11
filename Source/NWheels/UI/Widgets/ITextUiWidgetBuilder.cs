using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Elements;

namespace NWheels.UI.Widgets
{
    public interface ITextUiWidgetBuilder<TModel, TState> : IVisualUiElementBuilder<TModel, TState, ITextUiWidgetBuilder<TModel, TState>>
    {
    }
}
