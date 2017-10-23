using System;
using System.Diagnostics.CodeAnalysis;
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
        private readonly Func<IComponentContainer, TAdapterConfiguration> _configurationFactory;
        private readonly ConfiguratorAction _configurator;
        private TAdapterConfiguration _configuration;
        private Type _adapterComponentType = null;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        protected AdapterInjectionPort(
            IComponentContainerBuilder containerBuilder, 
            TAdapterConfiguration configuration)
        {
            _portKey = ++_s_lastPortKey;
            _componentContainer = containerBuilder.AsInternal().RootContainer;
            _configuration = configuration;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        protected AdapterInjectionPort(
            IComponentContainerBuilder containerBuilder, 
            Func<IComponentContainer, TAdapterConfiguration> configurationFactory, 
            ConfiguratorAction configurator)
        {
            _portKey = ++_s_lastPortKey;
            _componentContainer = containerBuilder.AsInternal().RootContainer;
            _configurationFactory = configurationFactory;
            _configurator = configurator;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Configure(IComponentContainerBuilder newComponents)
        {
            if (_configuration == null && _configurationFactory != null)
            {
                _configuration = _configurationFactory(_componentContainer);
                _configurator?.Invoke(_configuration, _componentContainer, newComponents);
            }
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
            
            var registration = newComponents.RegisterAdapterComponentType<TAdapterInterface, TAdapterConfiguration>(this, adapterComponentType);
            CompleteAdapterComponentRegistration(registration);
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

        public Type AdapterInterfaceType => typeof(TAdapterInterface);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type AdapterConfigurationType => typeof(TAdapterConfiguration);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TAdapterConfiguration Configuration => _configuration;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type AdapterComponentType => _adapterComponentType;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ExcludeFromCodeCoverage]
        protected virtual void CompleteAdapterComponentRegistration(IComponentRegistrationBuilder registration)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public delegate void ConfiguratorAction(
            TAdapterConfiguration config, 
            IComponentContainer existingComponents, 
            IComponentContainerBuilder newComponents);
    }
}
