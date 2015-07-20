using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Gemini.Framework.Commands;
using Gemini.Framework.Services;
using Gemini.Framework.Threading;
using NWheels.Tools.TestBoard.Modules.StartPage;
using NWheels.Tools.TestBoard.Services;

namespace NWheels.Tools.TestBoard.Modules.ApplicationExplorer
{
    #region ViewApplicationExplorer

    [CommandDefinition]
    public class ViewApplicationExplorerCommandDefinition : CommandDefinition
    {
        public const string CommandName = "Application.ViewExplorer";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string Name
        {
            get { return CommandName; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string Text
        {
            get { return "Application Explorer"; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string ToolTip
        {
            get { return "Open Application Explorer tool window"; }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [CommandHandler]
    public class ViewApplicationExplorerCommandHandler : CommandHandlerBase<ViewApplicationExplorerCommandDefinition>
    {
        private readonly IShell _shell;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ImportingConstructor]
        public ViewApplicationExplorerCommandHandler(IShell shell)
        {
            _shell = shell;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override Task Run(Command command)
        {
            _shell.ShowTool<IApplicationExplorer>();
            return TaskUtility.Completed;
        }
    }

    #endregion

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    #region StartApplicationCommandDefinition

    [CommandDefinition]
    public class StartApplicationCommandDefinition : CommandDefinition
    {
        public const string CommandName = "Application.Start";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string Name
        {
            get { return CommandName; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string Text
        {
            get { return "Start"; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string ToolTip
        {
            get { return "Start current application"; }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [CommandHandler]
    public class StartApplicationCommandHandler : CommandHandlerBase<StartApplicationCommandDefinition>
    {
        private readonly IApplicationControllerService _controller;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ImportingConstructor]
        public StartApplicationCommandHandler(IApplicationControllerService controller)
        {
            _controller = controller;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Update(Command command)
        {
            command.Enabled = _controller.CanStart();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override Task Run(Command command)
        {
            return _controller.StartAsync();
        }
    }

    #endregion

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    #region StopApplicationCommandDefinition

    [CommandDefinition]
    public class StopApplicationCommandDefinition : CommandDefinition
    {
        public const string CommandName = "Application.Stop";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string Name
        {
            get { return CommandName; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string Text
        {
            get { return "Stop"; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string ToolTip
        {
            get { return "Stop current application"; }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [CommandHandler]
    public class StopApplicationCommandHandler : CommandHandlerBase<StopApplicationCommandDefinition>
    {
        private readonly IApplicationControllerService _controller;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ImportingConstructor]
        public StopApplicationCommandHandler(IApplicationControllerService controller)
        {
            _controller = controller;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Update(Command command)
        {
            command.Enabled = _controller.CanStop();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override Task Run(Command command)
        {
            return _controller.StopAsync();
        }
    }

    #endregion

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    #region LoadNewApplication

    [CommandDefinition]
    public class LoadNewApplicationCommandDefinition : CommandDefinition
    {
        public const string CommandName = "Application.LoadNew";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string Name
        {
            get { return CommandName; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string Text
        {
            get { return "Load..."; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string ToolTip
        {
            get { return "Choose an application to load"; }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [CommandHandler]
    public class LoadNewApplicationCommandHandler : CommandHandlerBase<LoadNewApplicationCommandDefinition>
    {
        private readonly IApplicationControllerService _controller;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ImportingConstructor]
        public LoadNewApplicationCommandHandler(IApplicationControllerService controller)
        {
            _controller = controller;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Update(Command command)
        {
            command.Enabled = _controller.CanLoad();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override Task Run(Command command)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.Title = "Load Appplication";
            dlg.DefaultExt = ".config";
            dlg.Filter = "Boot Config Files (boot.config)|boot.config|All Config Files (*.config)|*.config|All Files (*.*)|*.*";

            if ( dlg.ShowDialog() == true )
            {
                _controller.LoadAsync(bootConfigFilePath: dlg.FileName);
            }

            return TaskUtility.Completed;
        }
    }

    #endregion

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    #region LoadRecentApplication

    [CommandDefinition]
    public class LoadRecentApplicationCommandDefinition : CommandListDefinition
    {
        public const string CommandName = "Application.LoadRecent";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string Name
        {
            get { return CommandName; }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [CommandHandler]
    public class LoadRecentApplicationCommandHandler : ICommandListHandler<LoadRecentApplicationCommandDefinition>
    {
        private readonly IApplicationControllerService _controller;
        private readonly IRecentAppListService _recentAppList;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ImportingConstructor]
        public LoadRecentApplicationCommandHandler(IApplicationControllerService controller, IRecentAppListService recentAppList)
        {
            _controller = controller;
            _recentAppList = recentAppList;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Populate(Command command, List<Command> commands)
        {
            foreach ( var app in _recentAppList.GetRecentApps() )
            {
                commands.Add(new Command(command.CommandDefinition) {
                    Text = app.BootConfigFilePath,
                    Tag = app.BootConfigFilePath
                });
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Task Run(Command command)
        {
            var bootConfigFilePath = (string)command.Tag;
            return _controller.LoadAsync(bootConfigFilePath);
        }
    }

    #endregion
    
    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    #region UnloadApplication

    [CommandDefinition]
    public class UnloadApplicationCommandDefinition : CommandDefinition
    {
        public const string CommandName = "Application.Unload";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string Name
        {
            get { return CommandName; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string Text
        {
            get { return "Unload"; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string ToolTip
        {
            get { return "Unload current application"; }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [CommandHandler]
    public class UnloadApplicationCommandHandler : CommandHandlerBase<UnloadApplicationCommandDefinition>
    {
        private readonly IApplicationControllerService _controller;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ImportingConstructor]
        public UnloadApplicationCommandHandler(IApplicationControllerService controller)
        {
            _controller = controller;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Update(Command command)
        {
            command.Enabled = _controller.CanUnload();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override Task Run(Command command)
        {
            return _controller.UnloadAsync();
        }
    }

    #endregion
}
