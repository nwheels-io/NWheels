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
        private readonly Func<IInvocationMessage, Task> _nextHandler;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public RestApiService(IEnumerable<IResourceHandler> resources, Func<IInvocationMessage, Task> nextHandler)
        {
            //TODO: catch duplicate key exception and throw a more informative one instead
            _handlerByUriPath = resources.ToImmutableDictionary(x => x.UriPath);

            _nextHandler = nextHandler;
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
        //            var method = context.Request.Method;

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

        public Task HandleHttpRequest(HttpContext context)
        {
            throw new NotImplementedException();
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

        TProtocolHandler IRestApiService.GetProtocolHandler<TProtocolHandler>(string uriPath)
        {
            throw new NotImplementedException();
        }

        TProtocolHandler IRestApiService.GetProtocolHandler<TProtocolHandler>(string uriPath, string protocolName)
        {
            throw new NotImplementedException();
        }

        bool IRestApiService.TryGetProtocolHandler<TProtocolHandler>(string uriPath, out TProtocolHandler protocol)
        {
            throw new NotImplementedException();
        }

        bool IRestApiService.TryGetProtocolHandler<TProtocolHandler>(string uriPath, string protocolName, out TProtocolHandler protocol)
        {
            throw new NotImplementedException();
        }
    }
}
