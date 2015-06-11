using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class DomainApiFaultAttribute : Attribute
    {
        public DomainApiFaultAttribute(Type enumType)
        {
            this.EnumType = enumType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type EnumType { get; private set; }
    }
}
