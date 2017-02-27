namespace NWheels.Microservices
{
    public interface ILifecycleListenerComponent
    {
        void MicroserviceLoading();

        void Load();

        void MicroserviceLoaded();

        void MicroserviceActivating();

        void Activate();

        void MicroserviceActivated();

        void MicroserviceMaybeDeactivating();

        void MayDeactivate();

        void MicroserviceMaybeDeactivated();

        void MicroserviceMaybeUnloading();

        void MayUnload();

        void MicroserviceMaybeUnloaded();
    }
}
