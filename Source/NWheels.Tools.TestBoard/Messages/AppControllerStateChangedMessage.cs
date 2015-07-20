using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Tools.TestBoard.Modules.ApplicationExplorer;

namespace NWheels.Tools.TestBoard.Messages
{
    public class AppControllerStateChangedMessage
    {
        public AppControllerStateChangedMessage(ApplicationState newState)
        {
            this.NewState = newState;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ApplicationState NewState { get; private set; }
    }
}
