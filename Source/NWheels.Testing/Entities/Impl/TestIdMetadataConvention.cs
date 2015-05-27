using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects.Core.Conventions;
using NWheels.DataObjects.Core;

namespace NWheels.Testing.Entities.Impl
{
    public class TestIdMetadataConvention  : IMetadataConvention
    {
        public void InjectCache(TypeMetadataCache cache)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Preview(TypeMetadataBuilder type)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Apply(TypeMetadataBuilder type)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public void Finalize(TypeMetadataBuilder type)
        {
            if ( type.PrimaryKey != null )
            {
                foreach ( var keyProperty in type.PrimaryKey.Properties )
                {
                    if ( keyProperty.DefaultValueGeneratorType == null && keyProperty.ClrType == typeof(int) )
                    {
                        keyProperty.DefaultValueGeneratorType = typeof(TestIntIdValueGenerator);
                    }
                }
            }
        }
    }
}
