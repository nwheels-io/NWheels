using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Entities.Metadata
{
    public interface IEntityRelationMetadata
    {
        EntityRelationKind RelationKind { get; }
        EntityRelationPartyKind ThisEntityPartyKind { get; }
        IEntityKeyMetadata ThisEntityKey { get; }
        IEntityMetadata RelatedEntity { get; }
        EntityRelationPartyKind RelatedEntityPartyKind { get; }
        IEntityKeyMetadata RelatedEntityKey { get; }
    }
}
