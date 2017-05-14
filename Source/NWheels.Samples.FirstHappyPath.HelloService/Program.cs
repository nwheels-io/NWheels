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
                .UseRest()
                .UseMessageProtocol<HttpRestNWheelsV1Protocol>()
                .UseRestApiHttpEndpoint<HttpRestNWheelsV1Protocol>(listenPortNumber: 5000)
                //.UseWebAppHttpEndpoint<HelloWorldApp>(listenPortNumber: 5500)
                .ContributeComponents((existingComponents, newComponents) => {
                    newComponents.ContributeTransactionScript<HelloWorldTx>();
                })
                .UseApplicationFeature<GeneratedCodePrototypesFeatureLoader>()
                .Build();

            return microservice.Run(args);
        }
    }
}