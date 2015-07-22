using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Samples.SimpleChatApp
{
    public interface IChatService
    {
        void StartListening();
        void Shutdown();
    }
}
