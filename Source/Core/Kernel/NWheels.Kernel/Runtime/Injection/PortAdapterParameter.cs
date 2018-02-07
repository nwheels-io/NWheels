using Autofac;
using Autofac.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NWheels.Kernel.Api.Injection;
using System.Reflection;

namespace NWheels.Kernel.Runtime.Injection
{
    public static class PortAdapterParameter
    {
        public static Autofac.Core.Parameter FromPort<TAdapter, TConfig>(AdapterInjectionPort<TAdapter, TConfig> port)
            where TAdapter : class
        {
            return new PortAdapterParameter<TAdapter, TConfig>(port);
        }
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    public class PortAdapterParameter<TAdapter, TConfig> : Autofac.Core.Parameter
        where TAdapter : class
    {
        public PortAdapterParameter(AdapterInjectionPort<TAdapter, TConfig> port)
        {
            this.PortType = port.GetType();
            this.PortKey = port.PortKey;
        }
        
        //-------------------------------------------------------------------------------------------------------------------------------------------------
        
        public override bool CanSupplyValue(ParameterInfo pi, IComponentContext context, out Func<object> valueProvider)
        {
            if (typeof(TAdapter).IsAssignableFrom(pi.ParameterType))
            {
                valueProvider = () => {
                    var port = (AdapterInjectionPort<TAdapter, TConfig>)context.ResolveKeyed(PortKey, PortType);
                    return port.Resolve();
                };
                return true;
            }
            valueProvider = null;
            return false;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------
        
        public Type PortType { get; }
        public int PortKey { get; }
    }
}
