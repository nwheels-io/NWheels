using System;
using NWheels.Kernel.Api.Extensions;

namespace NWheels.Kernel.Api.Injection
{
    public abstract class AdapterInjectionPort<TAdapterInterface, TAdapterConfiguration>
        where TAdapterInterface : class
    {
        private readonly IComponentContainer _componentContainer;
        private Type _adapterComponentType = null;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        protected AdapterInjectionPort(IComponentContainerBuilder containerBuilder, TAdapterConfiguration defaultConfiguration)
        {
            _componentContainer = containerBuilder.AsInternal().RootContainer;
            this.Configuration = defaultConfiguration;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Assign<TAdapterComponent>()
            where TAdapterComponent : class, TAdapterInterface
        {
            Assign(typeof(TAdapterComponent));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Assign(Type adapterComponentType)
        {
            if (adapterComponentType == null)
            {
                throw new ArgumentNullException(nameof(adapterComponentType));
            }

            if (_adapterComponentType != null)
            {
                throw new InvalidOperationException("Adapter component type is already assigned.");
            }

            _adapterComponentType = adapterComponentType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TAdapterInterface Resolve()
        {
            if (_adapterComponentType == null)
            {
                throw new InvalidOperationException("Adapter component type was not assigned.");
            }

            TAdapterInterface adapterInstance = (TAdapterInterface)_componentContainer.ResolveWithArguments(_adapterComponentType, this);
            return adapterInstance;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TAdapterConfiguration Configuration { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type AdapterComponentType => _adapterComponentType;
    }
}
