using NWheels.Frameworks.Ddd;
using NWheels.Injection.Adapters.Autofac;
using NWheels.Microservices;
using NWheels.Platform.Messaging;
using NWheels.Platform.Messaging.Adapters.AspNetKestrel;
using NWheels.Platform.Rest;
using System;

namespace NWheels.Samples.FirstHappyPath.HelloService
{
    class Program
    {
        public static int Main(string[] args)
        {
            var microservice = new MicroserviceHostBuilder("hello")
                .UseAutofac()
                .UseKestrel()
                .UseMessaging()
                .UseRestApi()
                .UseMessageProtocol<HttpRestNWheelsV1Protocol>()
                .ContributeComponents((existingComponents, newComponents) => {
                    newComponents.ContributeTransactionScript<HelloWorldTx>();
                    newComponents.ContributeHttpEndpoint("rest-api").ServeRestApiRequests<HttpRestNWheelsV1Protocol>();
                })
                .UseApplicationFeature<GeneratedCodeProrotypesFeatureLoader>()
                .Build();

            return microservice.Run(args);
        }
    }
}