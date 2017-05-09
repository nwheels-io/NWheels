using NWheels.Microservices;
using NWheels.Injection;
using System;
using NWheels.Samples.FirstHappyPath.Domain;
using NWheels.Frameworks.Ddd;
using NWheels.Platform.Messaging;
using NWheels.Platform.Rest;

namespace NWheels.Samples.FirstHappyPath
{
    [DefaultFeatureLoader]
    public class FirstHappyPathFeatureLoader : FeatureLoaderBase
    {
        public override void ContributeComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
        {
            newComponents
                .ContributeTransactionScript<HelloWorldTx>();

            newComponents
                .ContributeHttpEndpoint(name: "rest-api")
                .RouteRequestsToRestApiService(protocolName: MessageProtocolInfo.Select.HttpRestNWheelsV1().ProtocolName);
        }
    }
}
