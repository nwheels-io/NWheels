using NWheels.Injection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Execution
{
    public class DefaultInvocationScheduler : IInvocationScheduler
    {
        private readonly InvocationChannel _channel;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DefaultInvocationScheduler(IComponentContainer components)
        {
            _channel = new InvocationChannel(components);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IInvocationChannel GetInvocationChannel(string[] invocationTraits, string[] processorTraits)
        {
            return _channel;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class InvocationChannel : IInvocationChannel
        {
            private readonly IComponentContainer _components;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public InvocationChannel(IComponentContainer components)
            {
                _components = components;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Task ScheduledInvoke(IInvocationMessage invocation)
            {
                object target = null;// TODO: _components.Resolve(invocation.TargetType)
                return invocation.Invoke(target);
            }
        }
    }
}
