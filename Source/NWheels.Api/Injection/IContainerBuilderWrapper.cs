using NWheels.Microservices;

namespace NWheels.Injection
{
    public interface IContainerBuilderWrapper
    {
        void ContributeLifecycleListener<T>() where T : ILifecycleListenerComponent;

        void Register<TInterface, TImplementation>(LifeStyle lifeStyle = LifeStyle.Singleton) where TImplementation : TInterface;

        IContainerWrapper CreateContainer();
    }
}
