using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Elements;
using NWheels.UI.Templates;

namespace NWheels.UI.Widgets
{
    public interface ILogoUiWidgetBuilder<TModel, TState> : IVisualUiElementBuilder<TModel, TState, ILogoUiWidgetBuilder<TModel, TState>>
    {
        ILogoUiWidgetBuilder<TModel, TState> Image(string imagePath);
        ILogoUiWidgetBuilder<TModel, TState> Image(Expression<Func<IUiScope<TModel, TState>, string>> boundImagePath);
        ILogoUiWidgetBuilder<TModel, TState> MainTitle(string mainTitleText);
        ILogoUiWidgetBuilder<TModel, TState> MainTitle(Expression<Func<IUiScope<TModel, TState>, string>> boundMainTitleText);
        ILogoUiWidgetBuilder<TModel, TState> SubTitle(string subTitleText);
        ILogoUiWidgetBuilder<TModel, TState> SubTitle(Expression<Func<IUiScope<TModel, TState>, string>> boundSubTitleText);
    }
}
