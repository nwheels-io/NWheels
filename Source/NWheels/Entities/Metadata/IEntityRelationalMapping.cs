using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Entities.Metadata
{
    public interface IEntityRelationalMapping
    {
        string PrimaryTableName { get; }
        EntityRelationalInheritanceKind? InheritanceKind { get; }
    }
}
