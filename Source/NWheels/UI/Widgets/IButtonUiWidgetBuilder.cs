using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Behaviors;

namespace NWheels.UI.Widgets
{
    public interface IButtonUiWidgetBuilder<TModel, TState> : IBoundUiElementBuilder<TModel, TState>
    {
        IButtonUiWidgetBuilder<TModel, TState> BindToCommand(IUiCommandBuilder command);
    }
}
