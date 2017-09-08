using NWheels.Injection;

namespace NWheels.Injection
{
    public interface IInternalComponentContainerBuilder : IComponentContainerBuilder
    {
        IInternalComponentContainer CreateComponentContainer(bool isRootContainer); //TODO: remove isRootContainer parameter - determine according to root container field
        IInternalComponentContainer RootContainer { get; }
    }
}
