using Microsoft.AspNetCore.Http;
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

        TProtocolHandler GetProtocolHandler<TProtocolHandler>(string uriPath)
            where TProtocolHandler : class, IResourceProtocolHandler;

        TProtocolHandler GetProtocolHandler<TProtocolHandler>(string uriPath, string protocolName)
            where TProtocolHandler : class, IResourceProtocolHandler;

        bool TryGetProtocolHandler<TProtocolHandler>(string uriPath, out TProtocolHandler protocol)
            where TProtocolHandler : class, IResourceProtocolHandler;

        bool TryGetProtocolHandler<TProtocolHandler>(string uriPath, string protocolName, out TProtocolHandler protocol)
            where TProtocolHandler : class, IResourceProtocolHandler;
    }
}
