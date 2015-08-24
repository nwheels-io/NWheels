using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Configuration
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public class ConfiguredComponentAttribute : Attribute
    {
        public ConfiguredComponentAttribute(Type componentType)
        {
            this.ComponentType = componentType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type ComponentType { get; private set; }
    }
}
