using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Processing;

namespace NWheels.Stacks.UI.WpfCaliburnAvalon.CqrsApp
{
    public class CommandResult
    {
        private readonly Action<CommandResult> _onCompleted;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal CommandResult(IServerCommand command, Action<CommandResult> onCompleted)
        {
            Command = command;
            _onCompleted = onCompleted;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IServerCommand Command { get; private set; }
        public ICommandProcessedEvent Completion { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal void NotifyCompleted(ICommandProcessedEvent completionEvent)
        {
            this.Completion = completionEvent;

            if (_onCompleted != null)
            {
                _onCompleted(this);
            }
        }
    }
}
