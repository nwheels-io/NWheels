using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.DataObjects
{
    public interface ITypeMetadataCache
    {
        ITypeMetadata GetTypeMetadata(Type contract);
        bool ContainsTypeMetadata(Type contract);
        bool TryGetTypeMetadata(Type contract, out ITypeMetadata metadata);
        void EnsureRelationalMapping(ITypeMetadata type);
    }
}
