using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Authorization
{
    [AttributeUsage(AttributeTargets.Enum, AllowMultiple = false)]
    public class EnumClaimContractAttribute : Attribute
    {
        public EnumClaimContractAttribute(EnumClaimKind kind)
        {
            this.Kind = kind;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EnumClaimKind Kind { get; private set; }
    }
}
