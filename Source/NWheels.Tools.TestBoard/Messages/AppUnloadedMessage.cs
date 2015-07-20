using NWheels.Hosting;

namespace NWheels.Tools.TestBoard.Messages
{
    public class AppUnloadedMessage
    {
        public AppUnloadedMessage(string bootConfigFilePath, BootConfiguration bootConfig)
        {
            this.BootConfigFilePath = bootConfigFilePath;
            this.BootConfig = bootConfig;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string BootConfigFilePath { get; private set; }
        public BootConfiguration BootConfig { get; private set; }
    }
}
