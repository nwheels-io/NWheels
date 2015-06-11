using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    public interface ICommandWidget
    {
        ICommand Command { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public static class CommandWidgetExtensions
    {
        public static T Command<T>(this T widget, ICommand command) where T : ICommandWidget
        {
            widget.Command = command;
            return widget;
        }
    }
}
