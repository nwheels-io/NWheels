using System;
using System.Collections.Generic;
using NWheels.Processing;

namespace NWheels.Stacks.UI.WpfCaliburnAvalon.CqrsApp
{
    public interface ICqrsClient
    {
        void Connect(string userName, string password);
        void Disconnect();
        void SendCommands(IList<IServerCommand> commands);
        event Action<CommandResult> Connected;
        event Action<CommandResult> Disconnected;
        event Action<IList<IPushEvent>> EventsReceived;
    }
}
