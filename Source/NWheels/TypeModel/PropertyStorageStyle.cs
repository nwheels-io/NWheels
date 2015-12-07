using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Extensions;

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

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public static class PropertyStorageStyleExtensions
    {
        public static bool IsEmbeddedInParent(this PropertyStorageStyle value)
        {
            return value.IsIn(PropertyStorageStyle.InlineScalar, PropertyStorageStyle.EmbeddedObject, PropertyStorageStyle.EmbeddedObjectCollection);
        }
    }
}
