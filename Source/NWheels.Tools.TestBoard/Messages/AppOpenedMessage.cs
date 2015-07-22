using NWheels.Hosting;
using NWheels.Testing.Controllers;

namespace NWheels.Tools.TestBoard.Messages
{
    public class AppOpenedMessage
    {
        public AppOpenedMessage(ApplicationController app)
        {
            this.App = app;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ApplicationController App { get; private set; }
    }
}
