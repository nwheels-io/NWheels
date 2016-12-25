using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Endpoints;
using NWheels.Logging;

namespace NWheels.Samples.SimpleChat.Contracts
{
    public interface IChatClientApi
    {
        void PushServerInfo(ServerInfo info);
        Task<string> RequestPassword();
        void PushMessage(string who, string what);
    }
}
