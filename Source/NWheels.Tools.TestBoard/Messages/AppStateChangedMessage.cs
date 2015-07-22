using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Testing.Controllers;
using NWheels.Tools.TestBoard.Modules.ApplicationExplorer;

namespace NWheels.Tools.TestBoard.Messages
{
    public class AppStateChangedMessage
    {
        public AppStateChangedMessage(ApplicationController app)
        {
            this.App = app;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ApplicationController App { get; private set; }
    }
}
