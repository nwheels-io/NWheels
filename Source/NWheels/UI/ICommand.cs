using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    public interface ICommand : IUIElement
    {
        string Icon { get; set; }
        string Text { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public static class CommandFluentApi
    {
        public static ICommand Icon(this ICommand command, string value)
        {
            command.Icon = value;
            return command;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ICommand Title(this ICommand command, string value)
        {
            command.Icon = value;
            return command;
        }
    }
}
