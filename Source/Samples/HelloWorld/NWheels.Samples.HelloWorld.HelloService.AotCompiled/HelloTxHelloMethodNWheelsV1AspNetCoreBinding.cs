using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using NWheels.RestApi.Api;

namespace NWheels.Samples.HelloWorld.HelloService.AotCompiled
{
    [GeneratedCode(tool: "NWheels", version: "0.1.0-0.dev.1")]
    public class HelloTxHelloMethodNWheelsV1AspNetCoreBinding : IResourceBinding<HttpContext>
    {
        private static readonly string _s_protocol = "nwheels.v1";
        private static readonly string _s_uriPath = "/api/tx/Hello/Hello";
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private readonly HelloTxHelloMethodResourceHandler _handler;
        private readonly IRestApiLogger _logger;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public HelloTxHelloMethodNWheelsV1AspNetCoreBinding(
            HelloTxHelloMethodResourceHandler handler, 
            IRestApiLogger logger)
        {
            _handler = handler;
            _logger = logger;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public async Task HandleRequest(HttpContext context)
        {
            if (context.Request.Method.Equals(HttpMethod.Post.Method, StringComparison.InvariantCultureIgnoreCase))
            {
                var invocation = new HelloTxHelloMethodInvocation();

                try
                {
                    using (var reader = new StreamReader(context.Request.Body))
                    {
                        using (var json = new JsonTextReader(reader))
                        {
                            if (!json.Read() || !HelloTxHelloMethodInvocation.InputMessageSerializer.DeserializeFromJson(json, ref invocation.Input))
                            {
                                _logger.RestApiBadRequest(_handler.UriPath, context.Request.Method);
                                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                                return;
                            }
                        }
                    }
                }
                catch (Exception error)
                {
                    _logger.RestApiBadRequest(_handler.UriPath, context.Request.Method, error);
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                }

                try
                {
                    await _handler.PostNew(invocation);

                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "application/json";

                    using (var writer = new StreamWriter(context.Response.Body))
                    {
                        writer.Write($"{{\"result\": {JsonConvert.ToString(invocation.Output.ReturnValue)}}}");
                        writer.Flush();
                    }
                    
                    _logger.RestApiRequestCompleted(_handler.UriPath, context.Request.Method);
                }
                catch (Exception e)
                {
                    _logger.RestApiRequestFailed(_handler.UriPath, context.Request.Method, error: e);
                    context.Response.StatusCode = 500;
                }
            }
            else
            {
                _logger.RestApiBadRequest(_handler.UriPath, context.Request.Method);
                context.Response.StatusCode = 400;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public IResourceDescription Resource => _handler;
        public string UriPath => _s_uriPath;
        public string Protocol => _s_protocol;
    }
}
