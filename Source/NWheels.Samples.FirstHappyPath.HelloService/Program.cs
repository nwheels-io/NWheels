using NWheels.Frameworks.Ddd;
using NWheels.Frameworks.Uidl;
using NWheels.Frameworks.Uidl.Adapters.WebAngular;
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
                .UseDefaultWebStack(listenPortNumber: 5000)
                .AutoDiscoverComponents()
                .Build();

            return microservice.Run(args);
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    // the following is demo mockup code which should have real implementation in core and technology stack modules
    public static class MicroserviceHostBuilderExtensions
    {
        public static IMicroserviceHostBuilder UseDefaultWebStack(this IMicroserviceHostBuilder builder, int listenPortNumber)
        {
            return builder
                .UseAutofac()
                .UseKestrel()
                .UseAngular()
                .UseMessaging()
                .UseRest()
                .UseUidl()
                .UseMessageProtocol<HttpRestNWheelsV1Protocol>()
                .UseRestApiHttpEndpoint<HttpRestNWheelsV1Protocol>(listenPortNumber);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IMicroserviceHostBuilder AutoDiscoverComponents(this IMicroserviceHostBuilder builder)
        {
            return builder
                .ContributeComponents((existingComponents, newComponents) => {
                    newComponents.ContributeTransactionScript<HelloWorldTx>();
                    newComponents.ContributeWebApp<HelloWorldApp>(urlPathBase: "/");
                })
                .UseApplicationFeature<GeneratedCodePrototypesFeatureLoader>();
        }
    }
}