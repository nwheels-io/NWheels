namespace NWheels.Kernel.Api.Injection
{
    public abstract class InjectorPort
    {
        protected InjectorPort(IComponentContainerBuilder containerBuilder)
        {
            this.Components = ((IInternalComponentContainerBuilder)containerBuilder).RootContainer;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IComponentContainer Components { get; }
    }
}
