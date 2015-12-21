using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Authorization;
using NWheels.Processing.Messages;

namespace NWheels.Endpoints.Core
{
    public interface IEndpoint
    {
        void PushMessage(ISession session, IMessageObject message);
        string Name { get; }
        bool IsPushSupported { get; }
        TimeSpan? SessionIdleTimeoutDefault { get; }
    }
}
