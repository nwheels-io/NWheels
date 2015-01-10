using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Behaviors;

namespace NWheels.UI.Widgets
{
    public interface IFormUiWidgetBuilder<TModel, TState> : IBoundUiElementBuilder<TModel, TState>
    {
        IFormUiWidgetBuilder<TModel, TState> LabelsOnLeft();
        IFormUiWidgetBuilder<TModel, TState> LabelsOnTop();
        IFormUiWidgetBuilder<TModel, TState> SubmitButton(string text);
        IUiBehaviorSelector<TModel, TState, Unbound.Input> SubmitBehavior();
        IFormUiWidgetBuilder<TModel, TState> ResetButton(string text);
        IUiBehaviorSelector<TModel, TState, Unbound.Input> ResetBehavior();
    }
}
