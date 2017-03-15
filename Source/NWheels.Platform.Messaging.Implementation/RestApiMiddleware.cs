using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace NWheels.Platform.Messaging
{
    public class RestApiMiddleware
    {
        private readonly RequestDelegate _next;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public RestApiMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public async Task Invoke(HttpContext httpContext)
        {
            await httpContext.Response.WriteAsync("Hello from Middleware");
            await _next.Invoke(httpContext);
        }
    }
}
