namespace NWheels.Injection
{
    public interface IInternalComponentContainerBuilder : IComponentContainerBuilder
    {
        IInternalComponentContainer CreateComponentContainer(bool isRootContainer);
        IInternalComponentContainer RootContainer { get; }
    }
}
