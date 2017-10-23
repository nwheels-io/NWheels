using System;
using System.Threading.Tasks;
using NWheels.Microservices.Api;
using NWheels.Communication.Api.Extensions;
using Microsoft.AspNetCore.Http;
using NWheels.Communication.Api;
using System.Linq;
using System.IO;
using Microsoft.Extensions.Primitives;
using NWheels.Communication.Api.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;
using NWheels.Communication.Adapters.AspNetCore.Api;

namespace NWheels.Samples.HelloWorld.HelloService
{
    public class Program
    {
        static int Main(string[] args)
        {
            var cli = new MicroserviceHostBuilder("HelloService")
                .UseLifecycleComponent<HelloComponent>()
                .UseHttpEndpoint(configure: endpoint => {
                    endpoint.ListenOnPort(5000);
                    endpoint.HttpsListenOnPort(5001, "sslcert.pfx", "12345");
                    endpoint.StaticFolder("/files", localPath: new[] { "WebFiles" }, defaultFiles: new[] { "index.html" });
                    endpoint.Middleware<HelloMiddleware>();
                })
                .ContributeComponents((existingComponents, newComponents) => {
                    newComponents.RegisterComponentType<HelloTx>().InstancePerDependency();
                    newComponents.RegisterComponentType<TestHttpEndpointConfiguration>()
                        .InstancePerDependency()
                        .ForService<IHttpEndpointConfigElement>();
                })
                .UseAspNetCore()
                .BuildCli();

            return cli.Run(args);
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class HelloMiddleware : ICommunicationMiddleware<HttpContext>
        {
            private readonly Func<HelloTx> _tx;

            public HelloMiddleware(Func<HelloTx> tx)
            {
                this._tx = tx;
            }

            public async Task OnMessage(HttpContext context, Func<Task> next)
            {
                if (context.Request.Query.TryGetValue("name", out StringValues nameValues))
                {
                    var name = nameValues.FirstOrDefault();

                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        var tx = _tx();
                        var result = await tx.Hello(name);

                        context.Response.StatusCode = 200;
                        context.Response.ContentType = "application/json";

                        using (var writer = new StreamWriter(context.Response.Body))
                        {
                            writer.Write($"{{\"result\": \"{result}\"}}");
                            writer.Flush();
                        }

                        return;
                    }
                }

                context.Response.StatusCode = 400;
            }

            public void OnError(Exception error, Action next)
            {
                next?.Invoke();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class HelloComponent : LifecycleComponentBase
        {
            public override void MicroserviceActivated()
            {
                Console.WriteLine("HELLO WORLD!!!");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class HelloTx
        {
            public async Task<string> Hello(string name)
            {
                await Task.Yield();//Task.Delay(100);
                return $"Hello, {name}!";
            }
        }
    }
}
