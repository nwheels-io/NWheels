using Microsoft.AspNetCore.Http;
using NWheels.Platform.Messaging;
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
        private readonly ImmutableDictionary<ValueTuple<Type, string>, IMessageProtocolInterface> _protocolByTypeAndName;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected ResourceHandlerBase(string uriPath, IMessageProtocolInterface[] protocols)
        {
            _uriPath = uriPath;
            _protocolByTypeAndName = protocols.ToImmutableDictionary(
                p => new ValueTuple<Type, string>(
                    p.ProtocolInterface, 
                    p.ProtocolName));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string UriPath => _uriPath;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerable<IMessageProtocolInterface> GetAllProtocolHandlers()
        {
            return _protocolByTypeAndName.Values;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TProtocolInterface GetProtocol<TProtocolInterface>(string protocolName)
            where TProtocolInterface : class, IMessageProtocolInterface
        {
            if (TryGetProtocol<TProtocolInterface>(protocolName, out TProtocolInterface protocol))
            {
                return protocol;
            }

            throw new KeyNotFoundException(
                $"Protocol '{protocolName}':{typeof(TProtocolInterface).Name} is not implemented by resource '{_uriPath}'.");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool TryGetProtocol<TProtocolInterface>(string name, out TProtocolInterface protocol)
            where TProtocolInterface : class, IMessageProtocolInterface
        {
            var key = new ValueTuple<Type, string>(typeof(TProtocolInterface), name);
            if (_protocolByTypeAndName.TryGetValue(key, out IMessageProtocolInterface genericProtocol))
            {
                protocol = (TProtocolInterface)genericProtocol;
                return true;
            }

            protocol = null;
            return false;
        }
    }
}
