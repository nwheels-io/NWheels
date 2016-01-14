using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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
using NWheels.Testing.Controllers;

namespace NWheels.Tools.TestBoard.Modules.LogViewer
{
    #region ViewThreadLogs

    [CommandDefinition]
    public class ViewMongoDbThreadLogsCommandDefinition : CommandDefinition
    {
        public const string CommandName = "Log.ViewThreadLogs";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string Name
        {
            get { return CommandName; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string Text
        {
            get { return "Thread Logs from Mongo DB"; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string ToolTip
        {
            get { return "Open Thread Logs stored in Mongo databases"; }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [CommandHandler]
    public class ViewMongoDbThreadLogsCommandHandler : CommandHandlerBase<ViewMongoDbThreadLogsCommandDefinition>
    {
        private readonly IShell _shell;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ImportingConstructor]
        public ViewMongoDbThreadLogsCommandHandler(IShell shell)
        {
            _shell = shell;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override Task Run(Command command)
        {
            var existingLogDocument = _shell.Documents.OfType<LogViewerViewModel>().FirstOrDefault();

            if  ( existingLogDocument != null )
            {
                _shell.ActiveLayoutItem = existingLogDocument;
            }
            else
            {
                var connection = new MongoDbLogConnection(new[] { "server=localhost;database=nwheels_log" });
                _shell.OpenDocument(new LogViewerViewModel("from Mongo DB", new ILogConnection[] { connection }));
                _shell.OpenDocument(IoC.Get<LogViewerViewModel>());
            }

            return TaskUtility.Completed;
        }
    }

    #endregion
}
