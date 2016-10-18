using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using Autofac;

namespace NWheels.Stacks.Endpoints.Wcf.ServiceHostComponents
{
    public class InjectionEnabledInstanceProvider : IInstanceProvider
    {
        private Type _serviceContractType;
        private IComponentContext _components;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public InjectionEnabledInstanceProvider(Type serviceContractType, IComponentContext components)
        {
            _serviceContractType = serviceContractType;
            _components = components;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public object GetInstance(InstanceContext instanceContext)
        {
            return this.GetInstance(instanceContext, message: null);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            return _components.Resolve(_serviceContractType);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
        }
    }
}