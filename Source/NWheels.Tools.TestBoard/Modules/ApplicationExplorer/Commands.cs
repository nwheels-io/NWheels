using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
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
            //command.Enabled = _controller.CanStart();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override Task Run(Command command)
        {
            //return _controller.StartAsync();
            return TaskUtility.Completed;
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
            //command.Enabled = _controller.CanStop();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override Task Run(Command command)
        {
            //return _controller.StopAsync();
            return TaskUtility.Completed;
        }
    }

    #endregion

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    #region LoadNewApplication

    [CommandDefinition]
    public class OpenApplicationCommandDefinition : CommandDefinition
    {
        public const string CommandName = "Application.Open";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string Name
        {
            get { return CommandName; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string Text
        {
            get { return "Open..."; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override KeyGesture KeyGesture
        {
            get
            {
                return new KeyGesture(Key.O, ModifierKeys.Control);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string ToolTip
        {
            get { return "Open an application in Application Explorer"; }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [CommandHandler]
    public class OpenApplicationCommandHandler : CommandHandlerBase<OpenApplicationCommandDefinition>
    {
        private readonly IApplicationControllerService _controllerService;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ImportingConstructor]
        public OpenApplicationCommandHandler(IApplicationControllerService controller)
        {
            _controllerService = controller;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Update(Command command)
        {
            //command.Enabled = _controllerService.CanLoad();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override Task Run(Command command)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.Title = "Open Appplication";
            dlg.DefaultExt = ".config";
            dlg.Filter = "Boot Config Files (boot.config)|boot.config|All Config Files (*.config)|*.config|All Files (*.*)|*.*";

            if ( dlg.ShowDialog() == true )
            {
                _controllerService.Open(bootConfigFilePath: dlg.FileName);
            }

            return TaskUtility.Completed;
        }
    }

    #endregion

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    #region LoadRecentApplication

    [CommandDefinition]
    public class OpenRecentApplicationCommandDefinition : CommandListDefinition
    {
        public const string CommandName = "Application.OpenRecent";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string Name
        {
            get { return CommandName; }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [CommandHandler]
    public class OpenRecentApplicationCommandHandler : ICommandListHandler<OpenRecentApplicationCommandDefinition>
    {
        private readonly IApplicationControllerService _controller;
        private readonly IRecentAppListService _recentAppList;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ImportingConstructor]
        public OpenRecentApplicationCommandHandler(IApplicationControllerService controller, IRecentAppListService recentAppList)
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
            return Task.Run(() => _controller.Open(bootConfigFilePath));
        }
    }

    #endregion
    
    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    #region CloseAllApplications

    [CommandDefinition]
    public class CloseAllApplicationsCommandDefinition : CommandDefinition
    {
        public const string CommandName = "Application.CloseAll";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string Name
        {
            get { return CommandName; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string Text
        {
            get { return "Close All"; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string ToolTip
        {
            get { return "Close all applications"; }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [CommandHandler]
    public class CloseAllApplicationsCommandHandler : CommandHandlerBase<CloseAllApplicationsCommandDefinition>
    {
        private readonly IApplicationControllerService _controller;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ImportingConstructor]
        public CloseAllApplicationsCommandHandler(IApplicationControllerService controller)
        {
            _controller = controller;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Update(Command command)
        {
            command.Enabled = (_controller.Applications.Count > 0);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override Task Run(Command command)
        {
            return Task.Run(() => _controller.CloseAll());
        }
    }

    #endregion

    //---------------------------------------------------------------------------------------------------------------------------------------------------------
    
    #region BreakInDebugger

    [CommandDefinition]
    public class BreakInDebuggerCommandDefinition : CommandDefinition
    {
        public const string CommandName = "Application.BreakInDebugger";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string Name
        {
            get { return CommandName; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string Text
        {
            get { return "Break In Debugger"; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string ToolTip
        {
            get { return "Break In Debugger"; }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [CommandHandler]
    public class BreakInDebuggerCommandHandler : CommandHandlerBase<BreakInDebuggerCommandDefinition>
    {
        public override Task Run(Command command)
        {
            Debugger.Launch();
            return Task.FromResult(true);
        }
    }

    #endregion
}
