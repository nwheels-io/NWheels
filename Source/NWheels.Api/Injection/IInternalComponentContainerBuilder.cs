namespace NWheels.Injection
{
    public interface IInternalComponentContainerBuilder : IComponentContainerBuilder
    {
        IInternalComponentContainer CreateComponentContainer();
    }
}
