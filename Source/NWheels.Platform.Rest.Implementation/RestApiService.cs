using Microsoft.AspNetCore.Http;
using NWheels.Execution;
using NWheels.Injection;
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

        //public Task HandleHttpRequest(HttpContext context)
        //{
        //    IRestResourceHandler handler;

        //    try
        //    { 
        //        //TODO AbsolutePath will be replaced by Fragment or will be method's argument
        //        if (_handlerByUriPath.TryGetValue(context.Request.Path, out handler))
        //        {
        //            var method = context.Request.M ethod;

        //            if (method == HttpMethod.Get.Method)
        //            {
        //                return handler.HttpGet(context);
        //            }
        //            else if (method == HttpMethod.Post.Method)
        //            {
        //                return handler.HttpPost(context);
        //            }
        //            else if (method == HttpMethod.Put.Method)
        //            {
        //                return handler.HttpPut(context);
        //            }
        //            else if (method == _s_patchMethod.Method)
        //            {
        //                return handler.HttpPatch(context);
        //            }
        //            else if (method == HttpMethod.Delete.Method)
        //            {
        //                return handler.HttpDelete(context);
        //            }
        //            else
        //            {
        //                context.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
        //            }
        //        }
        //        else
        //        {
        //            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        //    }

        //    return Task.CompletedTask;
        //}

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

        public TProtocolHandler GetProtocolHandler<TProtocolHandler>(string uriPath)
            where TProtocolHandler : class, IResourceProtocolHandler
        {
            var handler = GetHandler<IResourceHandler>(uriPath);
            return handler.GetProtocolHandler<TProtocolHandler>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TProtocolHandler GetProtocolHandler<TProtocolHandler>(string uriPath, string protocolName)
            where TProtocolHandler : class, IResourceProtocolHandler
        {
            var handler = GetHandler<IResourceHandler>(uriPath);
            return handler.GetProtocolHandler<TProtocolHandler>(protocolName);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool TryGetProtocolHandler<TProtocolHandler>(string uriPath, out TProtocolHandler protocol)
            where TProtocolHandler : class, IResourceProtocolHandler
        {
            if (TryGetHandler<IResourceHandler>(uriPath, out IResourceHandler handler))
            {
                return handler.TryGetProtocolHandler<TProtocolHandler>(out protocol);
            }

            protocol = null;
            return false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool TryGetProtocolHandler<TProtocolHandler>(string uriPath, string protocolName, out TProtocolHandler protocol)
            where TProtocolHandler : class, IResourceProtocolHandler
        {
            if (TryGetHandler<IResourceHandler>(uriPath, out IResourceHandler handler))
            {
                return handler.TryGetProtocolHandler<TProtocolHandler>(protocolName, out protocol);
            }

            protocol = null;
            return false;
        }
    }
}
