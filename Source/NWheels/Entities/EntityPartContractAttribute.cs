using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Conventions;
using NWheels.DataObjects;

namespace NWheels.Entities
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public class EntityPartContractAttribute : DataObjectContractAttribute
    {
    }
}
