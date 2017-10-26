using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using NWheels.RestApi.Api;

namespace NWheels.Samples.HelloWorld.HelloService.AotCompiled
{
    public class HelloTxHelloMethodNWheelsV1AspNetCoreBinding : IResourceBinding<HttpContext>
    {
        private static readonly string _s_protocol = "nwheels.v1";
        private readonly HelloTxHelloMethodResourceHandler _handler;

        public HelloTxHelloMethodNWheelsV1AspNetCoreBinding(HelloTxHelloMethodResourceHandler handler)
        {
            _handler = handler;
        }

        public async Task HandleRequest(HttpContext context)
        {
            if (context.Request.Method.Equals("POST", StringComparison.InvariantCultureIgnoreCase))
            {
                var invocation = new HelloTxHelloMethodInvocation();

                if (context.Request.Query.TryGetValue("name", out StringValues nameValues))
                {
                    invocation.Input.Name = nameValues.ToString();
                }

                await _handler.PostNew(invocation);
                
                context.Response.StatusCode = 200;
                context.Response.ContentType = "application/json";

                using (var writer = new StreamWriter(context.Response.Body))
                {
                    writer.Write($"{{\"result\": \"{invocation.Output.ReturnValue}\"}}");
                    writer.Flush();
                }
            }
            else
            {
                context.Response.StatusCode = 400;
            }
        }

        public string Protocol => _s_protocol;
    }
}