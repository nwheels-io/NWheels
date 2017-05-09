using Microsoft.AspNetCore.Http;
using NWheels.Platform.Messaging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Platform.Rest
{
    public static class RestApiServiceExtensions
    {
        public static Task HandleHttpRequest(this IRestApiService restApiService, HttpContext context, string protocolName)
        {
            if (restApiService.TryGetHandlerProtocol<IHttpMessageProtocolInterface>(
                context.Request.Path, 
                protocolName,
                out IHttpMessageProtocolInterface protocol))
            {
                return protocol.HandleRequest(context);
            }

            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            return Task.CompletedTask;
        }
    }
}
