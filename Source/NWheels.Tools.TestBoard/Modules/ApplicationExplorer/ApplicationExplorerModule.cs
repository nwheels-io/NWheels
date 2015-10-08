using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Gemini.Framework;
using Gemini.Framework.Menus;
using Gemini.Framework.ToolBars;
using Gemini.Modules.PropertyGrid;
using NWheels.Tools.TestBoard.Modules.Main;

namespace NWheels.Tools.TestBoard.Modules.ApplicationExplorer
{
    [Export(typeof(IModule))]
    public class ApplicationExplorerModule : ModuleBase
    {
        public override IEnumerable<Type> DefaultTools
        {
            get { yield return typeof(IApplicationExplorer); }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Export]
        public static MenuItemDefinition ViewApplicationExplorerMenuItem =
            new CommandMenuItemDefinition<ViewApplicationExplorerCommandDefinition>(MainModule.ViewMenuGroup, 0);

        [Export]
        public static MenuItemDefinition ApplicationLoadMenuItem =
            new CommandMenuItemDefinition<OpenApplicationCommandDefinition>(MainModule.ApplicationMenuGroup, 0);

        [Export]
        public static MenuItemDefinition RecentAppsMenuItem =
            new TextMenuItemDefinition(MainModule.ApplicationMenuGroup, 1, "_Recent");

        [Export]
        public static MenuItemGroupDefinition RecentAppsMenuGroup =
            new MenuItemGroupDefinition(RecentAppsMenuItem, 1);

        [Export]
        public static MenuItemDefinition RecentAppMenuItemList = 
            new CommandMenuItemDefinition<OpenRecentApplicationCommandDefinition>(RecentAppsMenuGroup, 0);

        [Export]
        public static MenuItemGroupDefinition ApplicationControlMenuGroup =
            new MenuItemGroupDefinition(MainModule.ApplicationMenu, 2);

        [Export]
        public static MenuItemDefinition ApplicationStartMenuItem =
            new CommandMenuItemDefinition<StartApplicationCommandDefinition>(ApplicationControlMenuGroup, 0);

        [Export]
        public static MenuItemDefinition ApplicationStopMenuItem =
            new CommandMenuItemDefinition<StopApplicationCommandDefinition>(ApplicationControlMenuGroup, 1);

        [Export]
        public static MenuItemDefinition ApplicationCloseAllMenuItem =
            new CommandMenuItemDefinition<CloseAllApplicationsCommandDefinition>(ApplicationControlMenuGroup, 2);

        [Export]
        public static MenuItemDefinition BreakInDebuggerMenuItem =
            new CommandMenuItemDefinition<BreakInDebuggerCommandDefinition>(Gemini.Modules.MainMenu.MenuDefinitions.ToolsOptionsMenuGroup, 3);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Export]
        public static ToolBarItemGroupDefinition ApplicationExplorerToolBarGroup = 
            new ToolBarItemGroupDefinition(MainModule.MainToolBar, 10);

        [Export]
        public static ToolBarItemDefinition BreakInDebuggerToolBarItem = 
            new CommandToolBarItemDefinition<BreakInDebuggerCommandDefinition>(ApplicationExplorerToolBarGroup, 0);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        //[Export]
        //public static MenuItemGroupDefinition ViewMenuGroup =
        //    new MenuItemGroupDefinition(Gemini.Modules.MainMenu.MenuDefinitions.ViewMenu, 0);

        //[Export]
        //public static MenuItemDefinition ViewApplicationExplorerMenuItem =
        //    new CommandMenuItemDefinition<ViewApplicationExplorerCommandDefinition>(ViewMenuGroup, 0);
    }
}
