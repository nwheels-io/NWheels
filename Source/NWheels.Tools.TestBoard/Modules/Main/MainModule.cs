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
using NWheels.Hosting;
using NWheels.Testing.Controllers;
using NWheels.Tools.TestBoard.Messages;
using NWheels.Tools.TestBoard.Modules.ApplicationExplorer;
using NWheels.Tools.TestBoard.Properties;
using NWheels.Tools.TestBoard.Services;

namespace NWheels.Tools.TestBoard.Modules.Main
{
    [Export(typeof(IModule))]
    public class MainModule : 
        ModuleBase, 
        IHandle<AppOpenedMessage>, 
        IHandle<AppClosedMessage>,
        IHandle<AppStateChangedMessage>
    {
        public const string MainWindowTitle = "NWheels Test Board";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private readonly IMainWindow _mainWindow;
        private readonly IOutput _output;
        private readonly IEventAggregator _eventAggregator;
        private readonly IApplicationControllerService _controllerService;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ImportingConstructor]
        public MainModule(IMainWindow mainWindow, IOutput output, IEventAggregator eventAggregator, IApplicationControllerService controllerService)
        {
            _controllerService = controllerService;
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
            UpdateStatusBar(app: null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void PostInitialize()
        {
            base.PostInitialize();

            var commandLineArgs = Environment.GetCommandLineArgs();

            foreach ( var arg in commandLineArgs.Skip(1) )
            {
                _controllerService.Open(arg, autoRun: true);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IHandle<AppOpenedMessage>.Handle(AppOpenedMessage message)
        {
            UpdateWindowTitle();
            UpdateStatusBar(message.App);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IHandle<AppClosedMessage>.Handle(AppClosedMessage message)
        {
            UpdateWindowTitle();

            if ( _controllerService.Applications.Count == 0 )
            {
                UpdateStatusBar(null);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IHandle<AppStateChangedMessage>.Handle(AppStateChangedMessage message)
        {
            UpdateStatusBar(message.App);
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

        private void UpdateWindowTitle()
        {
            if ( _controllerService.Applications.Count == 0 )
            {
                _mainWindow.Title = MainWindowTitle;
            }
            else if ( _controllerService.Applications.Count == 1 )
            {
                _mainWindow.Title = string.Format("{0} - {1}", _controllerService.Applications[0].DisplayName, MainWindowTitle);
            }
            else
            {
                _mainWindow.Title = string.Format("{0} - {1}", "Multiple Apps", MainWindowTitle);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void UpdateStatusBar(ApplicationController app)
        {
            if ( app == null )
            {
                Shell.StatusBar.Items[0].Message = "Ready";
                Shell.StatusBar.Items[1].Message = "No App Loaded";
            }
            else
            {
                bool isReady = !app.CurrentState.IsIn(NodeState.Loading, NodeState.Activating, NodeState.Deactivating, NodeState.Unloading);

                Shell.StatusBar.Items[0].Message = (isReady ? "Ready" : app.CurrentState.ToString().SplitPascalCase().ToLower() + " application...");
                Shell.StatusBar.Items[1].Message = (isReady ? "App: " + app.CurrentState.ToString().SplitPascalCase() : "");
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
