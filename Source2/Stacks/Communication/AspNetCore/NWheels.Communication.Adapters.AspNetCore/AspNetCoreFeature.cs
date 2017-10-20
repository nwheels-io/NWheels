using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NWheels.Communication.Api;
using NWheels.Communication.Api.Http;
using NWheels.Kernel.Api.Injection;
using NWheels.Microservices.Api;

namespace NWheels.Communication.Adapters.AspNetCore
{
    public class AspNetCoreFeature : AdvancedFeature
    {
        public override void ContributeAdapterComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
        {
            var allAdapterPorts = existingComponents.ResolveAll<HttpEndpointAdapterInjectionPort>();

            foreach (var adapterPort in allAdapterPorts)
            {
                adapterPort.Assign<AspNetCoreHttpEndpoint>();
                newComponents.RegisterComponentType<AspNetCoreHttpEndpoint>()
                    .WithParameter(adapterPort)
                    .SingleInstance()
                    .ForServices<ICommunicationEndpoint, ILifecycleComponent>();
            }
        }
    }
}