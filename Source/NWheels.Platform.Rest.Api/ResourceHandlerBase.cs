using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Platform.Rest
{
    public abstract class ResourceHandlerBase : IResourceHandler
    {
        private readonly string _uriPath;
        //private readonly ImmutableDictionary<ValueTuple<Type,>, IRestApiProtocol> _protocolByName;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected ResourceHandlerBase(string uriPath, IResourceProtocolHandler[] protocols)
        {
            _uriPath = uriPath;
            //_protocolByName = protocols.ToImmutableDictionary(p => p.Name);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string UriPath => _uriPath;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerable<IResourceProtocolHandler> GetAllProtocolHandlers()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TProtocolHandler GetProtocolHandler<TProtocolHandler>()
            where TProtocolHandler : class, IResourceProtocolHandler
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TProtocolHandler GetProtocolHandler<TProtocolHandler>(string name)
            where TProtocolHandler : class, IResourceProtocolHandler
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool TryGetProtocolHandler<TProtocolHandler>(out TProtocolHandler handler)
            where TProtocolHandler : class, IResourceProtocolHandler
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool TryGetProtocolHandler<TProtocolHandler>(string name, out TProtocolHandler handler)
            where TProtocolHandler : class, IResourceProtocolHandler
        {
            throw new NotImplementedException();
            //if (_protocolByName.TryGetValue(name, out IRestApiProtocol anyProtocol))
            //{
            //    protocol = anyProtocol as TProtocol;
            //    return (protocol != null);
            //}

            //protocol = null;
            //return false;
        }
    }
}
