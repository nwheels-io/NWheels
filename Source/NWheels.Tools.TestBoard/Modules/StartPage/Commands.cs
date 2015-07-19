using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gemini.Framework.Commands;
using Gemini.Framework.Services;
using Gemini.Framework.Threading;
using NWheels.Tools.TestBoard.Modules.ApplicationExplorer;

namespace NWheels.Tools.TestBoard.Modules.StartPage
{
    #region ShowStartPage

    [CommandDefinition]
    public class ShowStartPageCommandDefinition : CommandDefinition
    {
        public const string CommandName = "StartPage.Show";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string Name
        {
            get { return CommandName; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string Text
        {
            get { return "Start Page"; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string ToolTip
        {
            get { return "Show the Start Page"; }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [CommandHandler]
    public class ShowStartPageCommandHandler : CommandHandlerBase<ShowStartPageCommandDefinition>
    {
        private readonly IShell _shell;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ImportingConstructor]
        public ShowStartPageCommandHandler(IShell shell)
        {
            _shell = shell;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override Task Run(Command command)
        {
            var existingStartPageDocument = _shell.Documents.OfType<StartPageViewModel>().FirstOrDefault();

            if ( existingStartPageDocument != null )
            {
                
            }

            return TaskUtility.Completed;
        }
    }

    #endregion
}
