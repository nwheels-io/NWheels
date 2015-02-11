using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Entities.Metadata
{
    public interface IEntityKeyMetadata
    {
        string Name { get; }
        EntityKeyKind Kind { get; }
        IReadOnlyList<IEntityPropertyMetadata> Properties { get; }
    }
}
