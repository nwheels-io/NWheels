using NWheels.Hosting;
using NWheels.Testing.Controllers;

namespace NWheels.Tools.TestBoard.Messages
{
    public class AppOpenedMessage
    {
        public AppOpenedMessage(ApplicationController app, bool autoRun)
        {
            this.App = app;
            this.AutoRun = autoRun;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ApplicationController App { get; private set; }
        public bool AutoRun { get; private set; }
    }
}
