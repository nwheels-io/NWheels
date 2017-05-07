using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Platform.Rest
{
    public static class RestApiServiceExtensions
    {
        public static Task HandleHttpRequest(this IRestApiService restApiService, HttpContext context)
        {
            if (restApiService.TryGetProtocolHandler<IHttpResourceProtocolHandler>(
                context.Request.Path, 
                out IHttpResourceProtocolHandler handler))
            {
                return handler.HandleHttpRequest(context);
            }

            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            return Task.CompletedTask;
        }
    }
}
