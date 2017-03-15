namespace NWheels.Injection
{
    public interface IInternalComponentContainerBuilder : IComponentContainerBuilder
    {
        IInternalComponentContainer CreateComponentContainer();

        void Register<TService>(object instance);
    }
}
