using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using Gemini.Framework;
using Gemini.Framework.Menus;
using Gemini.Framework.Services;
using Gemini.Modules.Inspector;
using Gemini.Modules.Output;
using Gemini.Modules.Output.Commands;
using NWheels.Extensions;
using NWheels.Tools.TestBoard.Messages;
using NWheels.Tools.TestBoard.Modules.ApplicationExplorer;
using NWheels.Tools.TestBoard.Properties;

namespace NWheels.Tools.TestBoard.Modules.Main
{
    [Export(typeof(IModule))]
    public class MainModule : 
        ModuleBase, 
        IHandle<AppLoadedMessage>, 
        IHandle<AppUnloadedMessage>,
        IHandle<AppControllerStateChangedMessage>
    {
        public const string MainWindowTitle = "NWheels Test Board";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private readonly IMainWindow _mainWindow;
        private readonly IOutput _output;
        private readonly IEventAggregator _eventAggregator;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ImportingConstructor]
        public MainModule(IMainWindow mainWindow, IOutput output, IEventAggregator eventAggregator)
        {
            _mainWindow = mainWindow;
            _output = output;
            _eventAggregator = eventAggregator;

            _eventAggregator.Subscribe(this);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Initialize()
        {
            Shell.ShowFloatingWindowsInTaskbar = true;
            Shell.ToolBars.Items.Clear();

            _mainWindow.Icon = ToImageSource(Resources.AppIcon);

            //MainWindow.WindowState = WindowState.Maximized;
            MainWindow.Title = MainWindowTitle;

            Shell.StatusBar.AddItem("", new GridLength(1, GridUnitType.Star));
            Shell.StatusBar.AddItem("", new GridLength(1, GridUnitType.Auto));
            Shell.StatusBar.AddItem("", new GridLength(25, GridUnitType.Pixel));

            _output.AppendLine(string.Format("Welcome to NWheels Test Board, version {0}", this.GetType().Assembly.GetName().Version));
            UpdateStatusBar(ApplicationState.NotLoaded);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IHandle<AppLoadedMessage>.Handle(AppLoadedMessage message)
        {
            _mainWindow.Title = string.Format("{0} - {1}", message.BootConfig.ApplicationName, MainWindowTitle);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IHandle<AppUnloadedMessage>.Handle(AppUnloadedMessage message)
        {
            _mainWindow.Title = MainWindowTitle;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IHandle<AppControllerStateChangedMessage>.Handle(AppControllerStateChangedMessage message)
        {
            UpdateStatusBar(message.NewState);
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

        private ImageSource ToImageSource(Icon icon)
        {
            ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return imageSource;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void UpdateStatusBar(ApplicationState state)
        {
            if (state == ApplicationState.NotLoaded)
            {
                Shell.StatusBar.Items[0].Message = "Ready";
                Shell.StatusBar.Items[1].Message = "No App Loaded";
            }
            else
            {
                bool isReady = !state.IsIn(ApplicationState.Loading, ApplicationState.Starting, ApplicationState.Stopping, ApplicationState.Unloading);

                Shell.StatusBar.Items[0].Message = (isReady ? "Ready" : state.ToString().SplitPascalCase().ToLower() + " application...");
                Shell.StatusBar.Items[1].Message = (isReady ? "App: " + state.ToString().SplitPascalCase() : "");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Export]
        public static MenuDefinition ApplicationMenu = 
            new MenuDefinition(Gemini.Modules.MainMenu.MenuDefinitions.MainMenuBar, 0, "_Application");

        [Export]
        public static MenuItemGroupDefinition ApplicationExitMenuGroup =
            new MenuItemGroupDefinition(MainModule.ApplicationMenu, 10);

        [Export]
        public static MenuItemGroupDefinition ApplicationMenuGroup =
            new MenuItemGroupDefinition(MainModule.ApplicationMenu, 0);

        [Export]
        public static MenuItemGroupDefinition ViewMenuGroup =
            new MenuItemGroupDefinition(Gemini.Modules.MainMenu.MenuDefinitions.ViewMenu, 0);

        [Export]
        public static MenuItemDefinition ViewOutputMenuItem = 
            new CommandMenuItemDefinition<ViewOutputCommandDefinition>(ViewMenuGroup, 10);

        [Export]
        public static MenuItemDefinition ApplicationExitMenuItem =
            new CommandMenuItemDefinition<Gemini.Modules.Shell.Commands.ExitCommandDefinition>(ApplicationExitMenuGroup, 0);

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
