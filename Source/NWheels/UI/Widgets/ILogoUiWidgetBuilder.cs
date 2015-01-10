using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Templates;

namespace NWheels.UI.Widgets
{
    public interface ILogoUiWidgetBuilder : IUiElementBuilder
    {
        ILogoUiWidgetBuilder Image(string imagePath);
        ILogoUiWidgetBuilder MainTitle(string mainTitleText);
        ILogoUiWidgetBuilder SubTitle(string subTitleText);
    }
}
