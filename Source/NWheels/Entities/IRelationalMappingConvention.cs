using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;

namespace NWheels.Entities
{
    public interface IRelationalMappingConvention
    {
        void ApplyToType(ITypeMetadata type);
    }
}
