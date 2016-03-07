using System;
using System.Collections.Generic;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Description;
using Autofac;
using Hapil;
using NWheels.Extensions;

namespace NWheels.Stacks.Endpoints.Wcf.ServiceHostComponents
{
    public class InjectionEnabledServiceHost : ServiceHostBase
    {
        private readonly IComponentContext _components;
        private readonly Type _serviceContractType;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public InjectionEnabledServiceHost(IComponentContext components, Type serviceContractType)
            : base()
        {
            _components = components;
            _serviceContractType = serviceContractType;

            InitializeDescription(new UriSchemeKeyedCollection());
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected override ServiceDescription CreateDescription(out IDictionary<string, ContractDescription> implementedContracts)
        {
            Type serviceType;

            if ( !_components.TryGetImplementationType(_serviceContractType, out serviceType) )
            {
                throw new ConfigurationErrorsException("Could not find registered implementation of contract: " + _serviceContractType.ToString());
            }

            var contractDescription = ContractDescription.GetContract(_serviceContractType);

            implementedContracts = new Dictionary<string, ContractDescription> {
                { contractDescription.ConfigurationName, contractDescription }
            };

            var serviceDescription = new ServiceDescription {
                ServiceType = serviceType,
                ConfigurationName = serviceType.FullName,
                Name = _serviceContractType.Name.TrimPrefix("I"),
                Namespace = _serviceContractType.Namespace,
            };

            serviceDescription.Behaviors.Add(new InjectionEnabledInstancingBehavior(_components, _serviceContractType));
                
            return serviceDescription;
        }
    }
}