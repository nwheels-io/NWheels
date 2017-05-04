using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Platform.Rest
{
    public interface IResourceHandler
    {
        TProtocolHandler GetProtocolHandler<TProtocolHandler>() 
            where TProtocolHandler : class, IResourceProtocolHandler;

        TProtocolHandler GetProtocolHandler<TProtocolHandler>(string name)
            where TProtocolHandler : class, IResourceProtocolHandler;

        bool TryGetProtocolHandler<TProtocolHandler>(out TProtocolHandler handler)
            where TProtocolHandler : class, IResourceProtocolHandler;

        bool TryGetProtocolHandler<TProtocolHandler>(string name, out TProtocolHandler handler)
            where TProtocolHandler : class, IResourceProtocolHandler;

        IEnumerable<IResourceProtocolHandler> GetAllProtocolHandlers();

        string UriPath { get; }
    }
}
