using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.Conventions;
using NWheels.Entities.Core;

namespace NWheels.Stacks.MongoDb
{
    public class ObjectIdGeneratorMetadataConvention : IMetadataConvention
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
                    if ( keyProperty.DefaultValueGeneratorType == null && keyProperty.ClrType == typeof(ObjectId) && !keyProperty.ContractPropertyInfo.CanWrite )
                    {
                        keyProperty.DefaultValueGeneratorType = typeof(ObjectIdPropertyValueGenerator);
                    }
                }
            }
        }
    }
}
