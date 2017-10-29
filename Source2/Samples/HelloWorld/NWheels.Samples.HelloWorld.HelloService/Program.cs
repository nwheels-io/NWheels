using System;
using System.Threading.Tasks;
using NWheels.Microservices.Api;
using NWheels.Communication.Api.Extensions;
using NWheels.Communication.Adapters.AspNetCore.Api;
using NWheels.RestApi.Api;
using NWheels.RestApi.Runtime;
using NWheels.RestApi.Runtime.Extensions;

namespace NWheels.Samples.HelloWorld.HelloService
{
    public class Program
    {
        static int Main(string[] args)
        {
            var cli = new MicroserviceHostBuilder("HelloService")
                .UseRestApiModel()
                .UseAspNetCoreAdapter()
                .UseHttpEndpoint(endpoint => endpoint
                    .Http(port: 5000)
                    .Https(port: 5001, certFilePath: "sslcert.pfx", certFilePassword: "12345")
                    .StaticFolder("/files", localPath: new[] { "WebFiles" }, defaultFiles: new[] { "index.html" })
                    .RestApiMiddleware<NWheelsV1Protocol>("/api/", resourceTypes: typeof(HelloTx))
                )
                .UseComponents((existingComponents, newComponents) => {
                    newComponents.RegisterComponentType<HelloTx>().InstancePerDependency();
                })
                .UseMicroserviceXml("microservice.xml")
                .BuildCli();

            return cli.Run(args);
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
