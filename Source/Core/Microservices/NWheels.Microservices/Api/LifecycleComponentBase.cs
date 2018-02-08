using System.Diagnostics.CodeAnalysis;

namespace NWheels.Microservices.Api
{
    [ExcludeFromCodeCoverage]
    public class LifecycleComponentBase : ILifecycleComponent
    {
        public virtual void MicroserviceLoading()
        {
        }

        public virtual void Load()
        {
        }

        public virtual void MicroserviceLoaded()
        {
        }

        public virtual void MicroserviceActivating()
        {
        }

        public virtual void Activate()
        {
        }

        public virtual void MicroserviceActivated()
        {
        }

        public virtual void MicroserviceMaybeDeactivating()
        {
        }

        public virtual void MayDeactivate()
        {
        }

        public virtual void MicroserviceMaybeDeactivated()
        {
        }

        public virtual void MicroserviceMaybeUnloading()
        {
        }

        public virtual void MayUnload()
        {
        }

        public virtual void MicroserviceMaybeUnloaded()
        {
        }
    }
}
