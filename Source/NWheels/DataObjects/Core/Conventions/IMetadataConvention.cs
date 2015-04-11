using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Core.DataObjects.Conventions
{
    public interface IMetadataConvention
    {
        void InjectCache(TypeMetadataCache cache);
        void Preview(TypeMetadataBuilder type);
        void Apply(TypeMetadataBuilder type);
        void Finalize(TypeMetadataBuilder type);
    }
}
