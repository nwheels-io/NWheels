using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.DataObjects
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class SemanticDataTypeAttribute : Attribute
    {
        public SemanticDataTypeAttribute(Type type)
        {
            this.Type = type;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type Type { get; private set; }
    }
}
