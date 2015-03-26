using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.DataObjects
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public class MustMixInAttribute : Attribute
    {
        public MustMixInAttribute(Type contractType)
        {
            this.ContractType = contractType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type ContractType { get; private set; }
    }
}
