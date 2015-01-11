using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Behaviors;
using System.Linq.Expressions;
using NWheels.UI.Elements;
using NWheels.UI.Layouts;

namespace NWheels.UI.Widgets
{
    public interface IFormUiWidgetBuilder<TModel, TState> : IUiLayoutBuilder<TModel, TState>
    {
        new IFormUiWidgetBuilder<TNewModel, TState> BindToModel<TNewModel>(Expression<Func<TModel, TNewModel>> path = null);
        new IFormUiWidgetBuilder<TModel, TNewState> BindToUiState<TNewState>(Expression<Func<TState, TNewState>> path = null);
        IFormUiWidgetBuilder<TModel, TState> LabelsOnLeft();
        IFormUiWidgetBuilder<TModel, TState> LabelsOnTop();
        IFormUiWidgetBuilder<TModel, TState> SubmitButton(IButtonUiWidgetBuilder button);
        IFormUiWidgetBuilder<TModel, TState> SubmitButton(string text, string imagePath = null);
        IFormUiWidgetBuilder<TModel, TState> SubmitBehavior(Action<IUiBehaviorSelector<TModel, TState, Unbound.Input>> definition);
        IFormUiWidgetBuilder<TModel, TState> ResetButton(IButtonUiWidgetBuilder button);
        IFormUiWidgetBuilder<TModel, TState> ResetButton(string text, string imagePath = null);
        IFormUiWidgetBuilder<TModel, TState> ResetBehavior(Action<IUiBehaviorSelector<TModel, TState, Unbound.Input>> definition);
    }
}
