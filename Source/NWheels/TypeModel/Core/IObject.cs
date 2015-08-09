using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.DataObjects.Core
{
    public interface IObject
    {
        Type ContractType { get; }
        Type FactoryType { get; }
    }
}
