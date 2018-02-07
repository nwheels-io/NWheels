using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Communication.Api
{
    public interface ICommunicationEndpoint
    {
        string Name { get; }
        string Protocol { get; }
        IEnumerable<string> ListenUrls { get; }    
        IEnumerable<ICommunicationMiddleware> Pipeline { get; }    
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface ICommunicationEndpoint<TMessageContext> : ICommunicationEndpoint
    {
        void AddMiddleware(ICommunicationMiddleware<TMessageContext> middleware);
    }
}
