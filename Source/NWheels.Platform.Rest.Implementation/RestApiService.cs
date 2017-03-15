using NWheels.Injection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

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

        public HttpResponseMessage HandleApiRequest(HttpRequestMessage request)
        {
            IRestResourceHandler handler;
            HttpResponseMessage response = null;

            try
            { 
                //TODO AbsolutePath will be replaced by Fragment or will be method's argument
                if (_handlerByUriPath.TryGetValue(request.RequestUri.AbsolutePath, out handler))
                {
                    if (request.Method == HttpMethod.Get)
                    {
                        response = handler.Get(request);
                    }
                    else if (request.Method == HttpMethod.Post)
                    {
                        response = handler.Post(request);
                    }
                    else if (request.Method == HttpMethod.Put)
                    {
                        response = handler.Put(request);
                    }
                    else if (request.Method == _s_patchMethod)
                    {
                        response = handler.Patch(request);
                    }
                    else if (request.Method == HttpMethod.Delete)
                    {
                        response = handler.Delete(request);
                    }
                    else
                    {
                        response = new HttpResponseMessage(HttpStatusCode.NotImplemented);
                    }
                }
                else
                {
                    response = new HttpResponseMessage(HttpStatusCode.NotFound);
                }
            }
            catch (Exception)
            {
                response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            return response;
        }
    }
}
