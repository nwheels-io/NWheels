using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Widgets
{
    public interface IDropDownMenuUiWidgetBuilder<TModel, TState> : IBoundUiElementBuilder<TModel, TState>
    {
        IDropDownMenuUiWidgetBuilder<TModel, TState> ButtonStyleGrip();
    }
}
