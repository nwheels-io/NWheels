using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NWheels.Injection.Mechanism
{
    public class ComponentRegistration
    {
        public ComponentRegistration()
        {
            this.ServiceTypes = new HashSet<Type>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type Type { get; set; }
        public object Instance { get; set; }
        public Func<IComponentContainer, object> InstanceFactory { get; set; }
        public string Key { get; set; }
        public HashSet<Type> ServiceTypes { get; private set; }
        public Type InstancingStrategyType { get; set; }
        public Type PrecedenceStrategyType { get; set; }
        public Type PipeStrategyType { get; set; }
        public ConstructorInfo Constructor { get; set; } 
        public ConstructorArgument[] ConstructorArguments { get; set; }
        public Dictionary<string, object> Metadata { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public struct ConstructorArgument
        {
            public ConstructorArgument(int index, object value)
            {
                this.Index = index;
                this.Value = value;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public readonly int Index;
            public readonly object Value;
        }
    }
}
