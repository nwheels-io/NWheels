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
    public class StartApplicationCommandHandler : CommandHandlerBase<StartApplicationCommandDefinition>
    {
        private readonly IApplicationExplorer _explorer;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ImportingConstructor]
        public StartApplicationCommandHandler(IApplicationExplorer explorer)
        {
            _explorer = explorer;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override Task Run(Command command)
        {
            return _explorer.Controller.StartAsync();
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
        private readonly IApplicationExplorer _explorer;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ImportingConstructor]
        public LoadNewApplicationCommandHandler(IApplicationExplorer explorer)
        {
            _explorer = explorer;
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
                MessageBox.Show("Now loading app from: " + dlg.FileName);
            }

            return TaskUtility.Completed;
        }
    }

    #endregion
}
