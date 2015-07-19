using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gemini.Framework.Commands;

namespace NWheels.Tools.TestBoard.Commands
{
    [CommandDefinition]
    public class ApplicationLoadCommandDefinition : CommandDefinition
    {
        public const string CommandName = "Application.Load";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string Name
        {
            get { return CommandName; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string Text
        {
            get { return "Load Application"; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string ToolTip
        {
            get { return "Load an application from boot.config"; }
        }
    }
}
