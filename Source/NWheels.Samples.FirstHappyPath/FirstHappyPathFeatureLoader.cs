using NWheels.Microservices;
using NWheels.Injection;
using System;
using NWheels.Samples.FirstHappyPath.Domain;
using NWheels.Frameworks.Ddd;

namespace NWheels.Samples.FirstHappyPath
{
    [DefaultFeatureLoader]
    public class FirstHappyPathFeatureLoader : FeatureLoaderBase
    {
        public override void ContributeComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
        {
            newComponents.ContributeTransactionScript<HelloWorldTx>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void CompileComponents(IComponentContainer input)
        {
        }
    }
}
