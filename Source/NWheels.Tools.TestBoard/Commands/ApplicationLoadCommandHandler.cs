using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Gemini.Framework;
using Gemini.Framework.Commands;
using Gemini.Framework.Services;
using Gemini.Framework.Threading;

namespace NWheels.Tools.TestBoard.Commands
{
    [CommandHandler]
    public class ApplicationLoadCommandHandler : CommandHandlerBase<ApplicationLoadCommandDefinition>
    {
        private readonly IShell _shell;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ImportingConstructor]
        public ApplicationLoadCommandHandler(IShell shell)
        {
            _shell = shell;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override Task Run(Command command)
        {

            //_shell. OpenDocument((IDocument)IoC.GetInstance(typeof(HelixViewModel), null));
            return TaskUtility.Completed;
        }
    }
}
