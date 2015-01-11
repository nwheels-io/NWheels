using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Behaviors;
using NWheels.UI.Elements;

namespace NWheels.UI.Widgets
{
    public interface IButtonUiWidgetBuilder
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IButtonUiWidgetBuilder<TModel, TState> : IButtonUiWidgetBuilder, IVisualUiElementBuilder<TModel, TState, IButtonUiWidgetBuilder<TModel, TState>>
    {
        IButtonUiWidgetBuilder<TModel, TState> BindToCommand(IUiCommandBuilder command);
    }
}
