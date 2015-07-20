using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Gemini.Framework;
using Gemini.Framework.Menus;
using Gemini.Framework.Services;
using Gemini.Modules.Inspector;
using Gemini.Modules.Output;
using Gemini.Modules.Output.Commands;
using NWheels.Tools.TestBoard.Modules.ApplicationExplorer;
using NWheels.Tools.TestBoard.Properties;

namespace NWheels.Tools.TestBoard.Modules.Main
{
    [Export(typeof(IModule))]
    public class MainModule : ModuleBase
    {
        private readonly IMainWindow _mainWindow;
        private readonly IOutput _output;
        private readonly IInspectorTool _inspectorTool;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ImportingConstructor]
        public MainModule(IMainWindow mainWindow, IOutput output, IInspectorTool inspectorTool)
        {
            _mainWindow = mainWindow;
            _output = output;
            _inspectorTool = inspectorTool;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Initialize()
        {
            Shell.ShowFloatingWindowsInTaskbar = true;
            Shell.ToolBars.Items.Clear();

            _mainWindow.Icon = ToImageSource(Resources.AppIcon);

            //MainWindow.WindowState = WindowState.Maximized;
            MainWindow.Title = "NWheels Test Board";

            Shell.StatusBar.AddItem("Ready", new GridLength(1, GridUnitType.Star));
            Shell.StatusBar.AddItem("No App Loaded", new GridLength(1, GridUnitType.Auto));
            Shell.StatusBar.AddItem("", new GridLength(25, GridUnitType.Pixel));

            _output.AppendLine("Started up");

            Shell.ActiveDocumentChanged += (sender, e) => RefreshInspector();
            RefreshInspector();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEnumerable<Type> DefaultTools
        {
            get
            {
                yield return typeof(IApplicationExplorer);
                yield return typeof(IOutput);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void RefreshInspector()
        {
            if ( Shell.ActiveItem != null )
            {
                _inspectorTool.SelectedObject =
                    new InspectableObjectBuilder().WithObjectProperties(Shell.ActiveItem, pd => pd.ComponentType == Shell.ActiveItem.GetType())
                        .ToInspectableObject();
            }
            else
            {
                _inspectorTool.SelectedObject = null;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private ImageSource ToImageSource(Icon icon)
        {
            ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return imageSource;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Export]
        public static MenuDefinition ApplicationMenu = 
            new MenuDefinition(Gemini.Modules.MainMenu.MenuDefinitions.MainMenuBar, 0, "_Application");

        [Export]
        public static MenuItemGroupDefinition ApplicationMenuGroup =
            new MenuItemGroupDefinition(MainModule.ApplicationMenu, 0);

        [Export]
        public static MenuItemGroupDefinition ViewMenuGroup =
            new MenuItemGroupDefinition(Gemini.Modules.MainMenu.MenuDefinitions.ViewMenu, 0);

        [Export]
        public static MenuItemDefinition ViewOutputMenuItem = 
            new CommandMenuItemDefinition<ViewOutputCommandDefinition>(ViewMenuGroup, 10);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Export]
        public static ExcludeMenuDefinition ExcludeFileMenu = 
            new ExcludeMenuDefinition(Gemini.Modules.MainMenu.MenuDefinitions.FileMenu);
        
        [Export]
        public static ExcludeMenuDefinition ExcludeEditMenu = 
            new ExcludeMenuDefinition(Gemini.Modules.MainMenu.MenuDefinitions.EditMenu);

        [Export]
        public static ExcludeMenuItemDefinition ExcludeViewErrorListMenuItem = 
            new ExcludeMenuItemDefinition(Gemini.Modules.ErrorList.MenuDefinitions.ViewErrorListMenuItem);

        [Export]
        public static ExcludeMenuItemDefinition ExcludeViewPropertyGridMenuItem =
            new ExcludeMenuItemDefinition(Gemini.Modules.PropertyGrid.MenuDefinitions.ViewPropertyGridMenuItem);

        [Export]
        public static ExcludeMenuItemDefinition ExcludeViewInspectorMenuItem =
            new ExcludeMenuItemDefinition(Gemini.Modules.Inspector.MenuDefinitions.ViewInspectorMenuItem);

        [Export]
        public static ExcludeMenuItemDefinition ExcludeViewToolboxMenuItem =
            new ExcludeMenuItemDefinition(Gemini.Modules.Toolbox.MenuDefinitions.ViewToolboxMenuItem);

        [Export]
        public static ExcludeMenuItemDefinition ExcludeViewHistoryMenuItem =
            new ExcludeMenuItemDefinition(Gemini.Modules.UndoRedo.MenuDefinitions.ViewHistoryMenuItem);

        [Export]
        public static ExcludeMenuItemGroupDefinition ExcludeViewOptionsMenuGroup =
            new ExcludeMenuItemGroupDefinition(Gemini.Modules.MainMenu.MenuDefinitions.ViewPropertiesMenuGroup);

        [Export]
        public static ExcludeMenuItemGroupDefinition ExcludeViewToolsMenuGroup =
            new ExcludeMenuItemGroupDefinition(Gemini.Modules.MainMenu.MenuDefinitions.ViewToolsMenuGroup);
    }
}
