using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Logging;

namespace NWheels.Samples.SimpleChatApp
{
    public interface IChatServiceLogger : IApplicationEventLogger
    {
        void LoginException(string username, Exception e);
    }
}
