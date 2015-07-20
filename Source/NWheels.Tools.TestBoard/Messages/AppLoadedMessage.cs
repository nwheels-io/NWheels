using NWheels.Hosting;

namespace NWheels.Tools.TestBoard.Messages
{
    public class AppLoadedMessage
    {
        public AppLoadedMessage(string bootConfigFilePath, BootConfiguration bootConfig)
        {
            this.BootConfigFilePath = bootConfigFilePath;
            this.BootConfig = bootConfig;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string BootConfigFilePath { get; private set; }
        public BootConfiguration BootConfig { get; private set; }
    }
}
