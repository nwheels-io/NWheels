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
        Task OnMessage(TMessageContext context, Func<Task> next);
        void OnError(Exception error, Action next);
    }
}
