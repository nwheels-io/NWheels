using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gemini.Framework;
using Gemini.Framework.Menus;
using NWheels.Tools.TestBoard.Modules.ApplicationExplorer;
using NWheels.Tools.TestBoard.Modules.Main;

namespace NWheels.Tools.TestBoard.Modules.LogViewer
{
    [Export(typeof(IModule))]
    public class LogViewerModule : ModuleBase
    {
        [Export]
        public static MenuItemDefinition ViewLogMenuItem =
            new CommandMenuItemDefinition<ViewMongoDbThreadLogsCommandDefinition>(MainModule.ViewMenuGroup, 0);
    }
}
