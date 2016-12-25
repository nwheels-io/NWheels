using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Endpoints;

namespace NWheels.Samples.SimpleChat.Contracts
{
    public interface IChatServiceApi
    {
        void RequestServerInfo();

        [NetworkApiContract.ConnectCommand]
        Task<UserInfo> Hello(string myName);

        [NetworkApiContract.DisconnectCommand]
        void GoodBye();

        void SayToOthers(string message);
    }
}
