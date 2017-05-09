using Microsoft.AspNetCore.Http;
using NWheels.Platform.Messaging;
using System.Net.Http;
using System.Threading.Tasks;

namespace NWheels.Platform.Rest
{
    public interface IRestApiService
    {
        THandler GetHandler<THandler>(string uriPath) 
            where THandler : class, IResourceHandler;

        bool TryGetHandler<THandler>(string uriPath, out THandler handler)
            where THandler : class, IResourceHandler;

        TProtocolInterface GetHandlerProtocol<TProtocolInterface>(string uriPath, string protocolName)
            where TProtocolInterface : class, IMessageProtocolInterface;

        bool TryGetHandlerProtocol<TProtocolInterface>(string uriPath, string protocolName, out TProtocolInterface protocol)
            where TProtocolInterface : class, IMessageProtocolInterface;
    }
}
