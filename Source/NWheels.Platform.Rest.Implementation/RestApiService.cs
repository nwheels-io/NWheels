using Microsoft.AspNetCore.Http;
using NWheels.Execution;
using NWheels.Injection;
using NWheels.Platform.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NWheels.Platform.Rest
{
    public class RestApiService : IRestApiService
    {
        private readonly ImmutableDictionary<string, IResourceHandler> _handlerByUriPath;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public RestApiService(IEnumerable<IResourceHandler> resources)
        {
            //TODO: catch duplicate key exception and throw a more informative one instead
            _handlerByUriPath = resources.ToImmutableDictionary(x => ensureLeadSlash(x.UriPath), x => x, StringComparer.OrdinalIgnoreCase);

            string ensureLeadSlash(string path)
            {
                if (path.Length > 0 && path[0] == '/')
                {
                    return path;
                }

                return ('/' + (path ?? string.Empty));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public THandler GetHandler<THandler>(string uriPath)
            where THandler : class, IResourceHandler
        {
            if (TryGetHandler<THandler>(uriPath, out THandler handler))
            {
                return handler;
            }

            throw new ArgumentException($"Handler for specified URI path does not exist: {uriPath}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool TryGetHandler<THandler>(string uriPath, out THandler handler)
            where THandler : class, IResourceHandler
        {
            if (_handlerByUriPath.TryGetValue(uriPath, out IResourceHandler nonTypedHandler))
            {
                handler = (THandler)nonTypedHandler;
                return true;
            }

            handler = null;
            return false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TProtocolInterface GetHandlerProtocol<TProtocolInterface>(string uriPath, string protocolName)
            where TProtocolInterface : class, IMessageProtocolInterface
        {
            var handler = GetHandler<IResourceHandler>(uriPath);
            return handler.GetProtocol<TProtocolInterface>(protocolName);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool TryGetHandlerProtocol<TProtocolInterface>(string uriPath, string protocolName, out TProtocolInterface protocol)
            where TProtocolInterface : class, IMessageProtocolInterface
        {
            if (TryGetHandler<IResourceHandler>(uriPath, out IResourceHandler handler))
            {
                return handler.TryGetProtocol<TProtocolInterface>(protocolName, out protocol);
            }

            protocol = null;
            return false;
        }
    }
}
