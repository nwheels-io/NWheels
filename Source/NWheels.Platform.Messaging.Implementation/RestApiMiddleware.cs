#if false

using Microsoft.AspNetCore.Http;
using NWheels.Platform.Rest;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NWheels.Platform.Messaging
{
    public class RestApiMiddleware// : IMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IRestApiService _restApiService;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public RestApiMiddleware(RequestDelegate next, IRestApiService restApiService)
        {
            _next = next;
            _restApiService = restApiService;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public async Task Invoke(HttpContext context)
        {
            var response = _restApiService.HandleApiRequest(new HttpRequestMessage()); 
            context.Response.StatusCode = (int)response.StatusCode;
            /*context.Response.StatusCode = (int)HttpStatusCode.OK;*/
            await context.Response.WriteAsync("Hello world!!!");
        }
    }
}

#endif