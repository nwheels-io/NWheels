using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Platform.Rest
{
    public abstract class ResourceHandlerBase : IResourceHandler
    {
        private readonly string _uriPath;
        private readonly ImmutableDictionary<ValueTuple<Type, string>, IResourceProtocolHandler> _protocolByTypeAndName;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected ResourceHandlerBase(string uriPath, IResourceProtocolHandler[] protocols)
        {
            _uriPath = uriPath;
            _protocolByTypeAndName = protocols.ToImmutableDictionary(
                p => new ValueTuple<Type, string>(
                    p.ProtocolInterface, 
                    p.Name ?? string.Empty));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string UriPath => _uriPath;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerable<IResourceProtocolHandler> GetAllProtocolHandlers()
        {
            return _protocolByTypeAndName.Values;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TProtocolHandler GetProtocolHandler<TProtocolHandler>()
            where TProtocolHandler : class, IResourceProtocolHandler
        {
            if (TryGetProtocolHandler<TProtocolHandler>(out TProtocolHandler handler))
            {
                return handler;
            }

            throw new KeyNotFoundException("Specified protocol handler does not exists.");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TProtocolHandler GetProtocolHandler<TProtocolHandler>(string name)
            where TProtocolHandler : class, IResourceProtocolHandler
        {
            if (TryGetProtocolHandler<TProtocolHandler>(name, out TProtocolHandler handler))
            {
                return handler;
            }

            throw new KeyNotFoundException("Specified protocol handler does not exists.");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool TryGetProtocolHandler<TProtocolHandler>(out TProtocolHandler handler)
            where TProtocolHandler : class, IResourceProtocolHandler
        {
            return TryGetProtocolHandler<TProtocolHandler>(string.Empty, out handler);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool TryGetProtocolHandler<TProtocolHandler>(string name, out TProtocolHandler handler)
            where TProtocolHandler : class, IResourceProtocolHandler
        {
            var key = new ValueTuple<Type, string>(typeof(TProtocolHandler), name);
            if (_protocolByTypeAndName.TryGetValue(key, out IResourceProtocolHandler genericHandler))
            {
                handler = (TProtocolHandler)genericHandler;
                return true;
            }

            handler = null;
            return false;
        }
    }
}
