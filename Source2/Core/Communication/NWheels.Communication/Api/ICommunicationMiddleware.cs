using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Communication.Api
{
    public interface ICommunicationMiddleware
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface ICommunicationMiddleware<TMessageContext> : ICommunicationMiddleware
    {
        Task OnMessage(TMessageContext context, ICommunicationMiddleware<TMessageContext> next);
        void OnError(Exception error, ICommunicationMiddleware<TMessageContext> next);
    }
}
