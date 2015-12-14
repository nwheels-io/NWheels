using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;

namespace NWheels.TypeModel
{
    public interface IDomainContextMetadata
    {
        Type ContractType { get; }
        Type GetImplementationTypeBy(Type facoryType);
        IEnumerable<Type> GetAllImplementations();
        IReadOnlyCollection<IDomainContextEntityMetadata> Entities { get; }
        IReadOnlyCollection<ITypeMetadata> Types { get; }
    }
}
