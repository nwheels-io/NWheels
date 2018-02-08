using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Communication.Api
{

    public interface ISessionCommunicationMiddleware<TMessageContext> : ICommunicationMiddleware<TMessageContext>
    {
        Task OnConnect(TMessageContext context, ISessionCommunicationMiddleware<TMessageContext> next);
        Task OnDisconnect(TMessageContext context, DisconnectReason reason, ISessionCommunicationMiddleware<TMessageContext> next);
    }
}
  