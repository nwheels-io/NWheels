using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.TypeModel
{
    public enum PropertyStorageStyle
    {
        Undefined,
        InlineScalar,
        InlineForeignKey,
        EmbeddedObject,
        EmbeddedObjectCollection,
        EmbeddedForeignKeyCollection,
        InverseForeignKey
    }
}
