using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Gemini.Framework;
using Gemini.Framework.Menus;
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
            new CommandMenuItemDefinition<LoadNewApplicationCommandDefinition>(MainModule.ApplicationMenuGroup, 0);

        [Export]
        public static MenuItemDefinition RecentAppsMenuItem =
            new TextMenuItemDefinition(MainModule.ApplicationMenuGroup, 1, "_Recent");

        [Export]
        public static MenuItemGroupDefinition RecentAppsMenuGroup =
            new MenuItemGroupDefinition(RecentAppsMenuItem, 1);

        [Export]
        public static MenuItemDefinition RecentAppMenuItemList = 
            new CommandMenuItemDefinition<LoadRecentApplicationCommandDefinition>(RecentAppsMenuGroup, 0);


        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        //[Export]
        //public static MenuItemGroupDefinition ViewMenuGroup =
        //    new MenuItemGroupDefinition(Gemini.Modules.MainMenu.MenuDefinitions.ViewMenu, 0);

        //[Export]
        //public static MenuItemDefinition ViewApplicationExplorerMenuItem =
        //    new CommandMenuItemDefinition<ViewApplicationExplorerCommandDefinition>(ViewMenuGroup, 0);
    }
}
