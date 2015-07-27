using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.DataObjects
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = true)]
    public class MustHaveMixinAttribute : Attribute
    {
        public MustHaveMixinAttribute(Type contractType)
        {
            this.ContractType = contractType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type ContractType { get; private set; }
    }
}
