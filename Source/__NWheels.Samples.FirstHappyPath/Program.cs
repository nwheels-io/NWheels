using NWheels.Microservices;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Samples.FirstHappyPath
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var microservice = new MicroserviceHostBuilder()
                .UseInjectionAdapterAutofac()
                .UsePlatformMessaging()
                .UsePlatformMessagingAdapterKestrel()
                .UsePlatformRest()
                .ContributeComponents((existingComponents, newComponents) => {
                    newComponents
                        .ContributeTransactionScript<HelloWorldTx>();
                    newComponents
                        .ContributeHttpEndpoint(name: "rest-api")
                        .RouteRequestsToRestApiService(protocolName: MessageProtocolInfo.Select.HttpRestNWheelsV1().ProtocolName);
                })
                .Build();

            return microservice.Run(args);
        }

        public class MicroserviceHostBuilder
        {
            public MicroserviceHostBuilder(string name)
            {
                BootConfig = new BootConfiguration();
                BootConfig.MicroserviceConfig.
            }

            public MicroserviceHost Build()
            {
                throw new NotImplementedException();
            }

            public BootConfiguration BootConfig { get; } 
        }
    }
}
