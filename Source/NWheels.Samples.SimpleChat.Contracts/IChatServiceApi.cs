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
        [NetworkApiContract.ConnectCommand]
        void Hello(string myName);

        [NetworkApiContract.DisconnectCommand]
        void GoodBye();

        void SayToOthers(string message);
    }
}
