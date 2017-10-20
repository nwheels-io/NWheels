using System;
using NWheels.Kernel.Api.Extensions;

namespace NWheels.Kernel.Api.Injection
{
    public abstract class AdapterInjectionPort<TAdapterInterface, TAdapterConfiguration> : IAdapterInjectionPort
        where TAdapterInterface : class
    {
        private static int _s_lastPortKey = 0;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private readonly int _portKey;
        private readonly IInternalComponentContainer _componentContainer;
        private readonly TAdapterConfiguration _configuration;
        private Type _adapterComponentType = null;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        protected AdapterInjectionPort(IComponentContainerBuilder containerBuilder, TAdapterConfiguration defaultConfiguration)
        {
            _portKey = ++_s_lastPortKey;
            _componentContainer = containerBuilder.AsInternal().RootContainer;
            _configuration = defaultConfiguration;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Assign<TAdapterComponent>(IComponentContainerBuilder newComponents)
            where TAdapterComponent : class, TAdapterInterface
        {
            Assign(typeof(TAdapterComponent), newComponents);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Assign(Type adapterComponentType, IComponentContainerBuilder newComponents)
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
            newComponents.RegisterAdapterComponentType<TAdapterInterface, TAdapterConfiguration>(this, adapterComponentType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TAdapterInterface Resolve()
        {
            if (_adapterComponentType == null)
            {
                throw new InvalidOperationException("Adapter component type was not assigned.");
            }

            var adapterInstance = _componentContainer.ResolveAdapter(this);
            return adapterInstance;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public int PortKey => _portKey;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TAdapterConfiguration Configuration => _configuration;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type AdapterInterfaceType => typeof(TAdapterInterface);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type AdapterConfigurationType => typeof(TAdapterConfiguration);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type AdapterComponentType => _adapterComponentType;
    }
}
