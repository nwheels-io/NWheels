namespace NWheels.Kernel.Api.Injection
{
    public interface IInternalComponentContainerBuilder : IComponentContainerBuilder
    {
        IInternalComponentContainer CreateComponentContainer(); 
        IInternalComponentContainer RootContainer { get; }
    }
}
