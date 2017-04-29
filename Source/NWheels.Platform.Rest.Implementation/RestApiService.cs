using Microsoft.AspNetCore.Http;
using NWheels.Injection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NWheels.Platform.Rest
{
    public class RestApiService : IRestApiService
    {
        private readonly Dictionary<string, IRestResourceHandler> _handlerByUriPath;
        private static readonly HttpMethod _s_patchMethod = new HttpMethod("PATCH");

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public RestApiService(IComponentContainer container)
        {
            _handlerByUriPath = container.ResolveAll<IRestResourceHandler>().ToDictionary(x => x.UriPath, x => x);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Task HandleHttpRequest(HttpContext context)
        {
            IRestResourceHandler handler;

            try
            { 
                //TODO AbsolutePath will be replaced by Fragment or will be method's argument
                if (_handlerByUriPath.TryGetValue(context.Request.Path, out handler))
                {
                    var method = context.Request.Method;

                    if (method == HttpMethod.Get.Method)
                    {
                        return handler.HttpGet(context);
                    }
                    else if (method == HttpMethod.Post.Method)
                    {
                        return handler.HttpPost(context);
                    }
                    else if (method == HttpMethod.Put.Method)
                    {
                        return handler.HttpPut(context);
                    }
                    else if (method == _s_patchMethod.Method)
                    {
                        return handler.HttpPatch(context);
                    }
                    else if (method == HttpMethod.Delete.Method)
                    {
                        return handler.HttpDelete(context);
                    }
                    else
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
                    }
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
            }
            catch (Exception)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }

            return Task.CompletedTask;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public THandler GetHandler<THandler>(string uriPath)
            where THandler : class, IRestResourceHandler
        {
            if (TryGetHandler<THandler>(uriPath, out THandler handler))
            {
                return handler;
            }

            throw new ArgumentException($"Handler for specified URI path does not exist: {uriPath}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool TryGetHandler<THandler>(string uriPath, out THandler handler)
            where THandler : class, IRestResourceHandler
        {
            if (_handlerByUriPath.TryGetValue(uriPath, out IRestResourceHandler nonTypedHandler))
            {
                handler = (THandler)nonTypedHandler;
                return true;
            }

            handler = null;
            return false;
        }
    }
}
