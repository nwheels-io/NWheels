using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Core;

namespace NWheels.UI
{
    /// <summary>
    /// Defines a widget, which is a basic element of user interaction.
    /// A Widget can declare contained widgets, notifications, and commands.
    /// </summary>
    public interface IWidget : IUIElement, IUIElementContainer, IDescriptionProvider<WidgetDescription>
    {
    }
}
