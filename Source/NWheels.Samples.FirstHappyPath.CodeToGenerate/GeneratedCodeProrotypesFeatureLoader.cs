using NWheels.Microservices;
using System;
using System.Collections.Generic;
using System.Text;
using NWheels.Injection;
using NWheels.Frameworks.Ddd.RestApi;
using NWheels.Platform.Rest;

namespace NWheels.Samples.FirstHappyPath.CodeToGenerate
{
    [DefaultFeatureLoader]
    public class GeneratedCodeProrotypesFeatureLoader : FeatureLoaderBase
    {
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
