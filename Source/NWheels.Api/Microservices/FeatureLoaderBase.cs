using NWheels.Injection;

namespace NWheels.Microservices
{
    public abstract class FeatureLoaderBase : IFeatureLoader
    {
        public virtual void RegisterConfigSections()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void RegisterComponents(IComponentContainerBuilder containerBuilder)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual void CompileComponents(IInternalComponentContainer input, IComponentContainerBuilder output)
        {
        }
    }
}
