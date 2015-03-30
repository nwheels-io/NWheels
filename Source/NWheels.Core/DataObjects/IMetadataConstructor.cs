using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;

namespace NWheels.Core.DataObjects
{
    public interface IMetadataConstructor<in TAttribute, in TMetadata>
        where TAttribute : Attribute
        where TMetadata : IMetadataElement
    {
        void ConstructMetadata(TAttribute attribute, TMetadata metadata);
    }
}
