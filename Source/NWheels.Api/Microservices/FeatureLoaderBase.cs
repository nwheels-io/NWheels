using System;
using NWheels.Injection;

namespace NWheels.Microservices
{
    public abstract class FeatureLoaderBase : IFeatureLoader
    {
        public virtual void ContributeConfigSections()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void ContributeComponents(IComponentContainerBuilder containerBuilder)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void ContributeAdapterComponents(IComponentContainer input, IComponentContainerBuilder output)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void CompileComponents(IComponentContainer input)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void ContributeCompiledComponents(IComponentContainer input, IComponentContainerBuilder output)
        {
        }
    }
}
