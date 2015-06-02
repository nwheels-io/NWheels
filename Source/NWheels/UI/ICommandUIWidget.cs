using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    public interface ICommandUIWidget
    {
        IUICommand Command { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public static class CommandUIWidgetExtensions
    {
        public static T Command<T>(this T widget, IUICommand command) where T : ICommandUIWidget
        {
            widget.Command = command;
            return widget;
        }
    }
}
