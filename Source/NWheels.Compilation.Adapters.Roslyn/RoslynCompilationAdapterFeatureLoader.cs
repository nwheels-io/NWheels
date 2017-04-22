using NWheels.Microservices;
using System;
using System.Collections.Generic;
using System.Text;
using NWheels.Injection;
using NWheels.Compilation.Mechanism.Factories;

namespace NWheels.Compilation.Adapters.Roslyn
{
    [DefaultFeatureLoader]
    public class RoslynCompilationAdapterFeatureLoader : FeatureLoaderBase
    {
        public override void ContributeComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
        {
            newComponents.RegisterComponentType<RoslynTypeFactoryBackend>()
                .SingleInstance()
                .ForService<ITypeFactoryBackend<IRuntimeTypeFactoryArtifact>>();

            //containerBuilder.Register<ITypeFactoryBackend<IRuntimeTypeFactoryArtifact>, RoslynTypeFactoryBackend>(LifeStyle.Singleton);
        }
    }
}
