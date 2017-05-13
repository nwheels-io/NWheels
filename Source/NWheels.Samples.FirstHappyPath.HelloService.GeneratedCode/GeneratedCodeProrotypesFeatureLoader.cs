using NWheels.Microservices;
using System;
using System.Collections.Generic;
using System.Text;
using NWheels.Injection;
using NWheels.Frameworks.Ddd.RestApi;
using NWheels.Platform.Rest;
using NWheels.Platform.Messaging;

namespace NWheels.Samples.FirstHappyPath.HelloService
{
    [DefaultFeatureLoader]
    public class GeneratedCodeProrotypesFeatureLoader : FeatureLoaderBase
    {
        public override void ContributeConfigSections(IComponentContainerBuilder newComponents)
        {
            base.ContributeConfigSections(newComponents);

            newComponents
                .RegisterComponentInstance(new ConfigSection_MessagingPlatformConfiguration())
                .ForService<IMessagingPlatformConfiguration>();
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public override void ContributeConfiguration(IComponentContainer existingComponents)
        {
            base.ContributeConfiguration(existingComponents);

            var messagingPlatformConfig = existingComponents.Resolve<IMessagingPlatformConfiguration>();
            var httpEndpointConfig = messagingPlatformConfig.NewHttpEndpointConfig();
            httpEndpointConfig.Name = "rest-api";
            httpEndpointConfig.Port = 5000;

            messagingPlatformConfig.Endpoints.Add("rest-api", httpEndpointConfig);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public override void ContributeCompiledComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
        {
            base.ContributeCompiledComponents(existingComponents, newComponents);

            var handlerList1 = new TxResourceHandlerList_of_HelloWorldTx(); // this instantation will be handled by TxResourceHandlerTypeFactory

            foreach (var handlerType in handlerList1.GetHandlerTypes())
            {
                newComponents.RegisterComponentType(handlerType).ForService<IResourceHandler>();
            }
        }
    }
}
