using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Entities.Metadata
{
    public enum EntityRelationKind
    {
        OneToOne,
        OneToMany,
        ManyToOne,
        ManyToMany
    }
}
