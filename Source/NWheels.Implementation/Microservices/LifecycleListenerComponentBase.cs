using System;

namespace NWheels.Microservices
{
    public class LifecycleListenerComponentBase : ILifecycleListenerComponent
    {
        public virtual void Activate()
        {
        }

        public virtual void Load()
        {
        }

        public virtual void MayDeactivate()
        {
        }

        public virtual void MayUnload()
        {
        }

        public virtual void MicroserviceActivated()
        {
        }

        public virtual void MicroserviceActivating()
        {
        }

        public virtual void MicroserviceLoaded()
        {
        }

        public virtual void MicroserviceLoading()
        {
        }

        public virtual void MicroserviceMaybeDeactivated()
        {
        }

        public virtual void MicroserviceMaybeDeactivating()
        {
        }

        public virtual void MicroserviceMaybeUnloaded()
        {
        }

        public virtual void MicroserviceMaybeUnloading()
        {
        }
    }
}
