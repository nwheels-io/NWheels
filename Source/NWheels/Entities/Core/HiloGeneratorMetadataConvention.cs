using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.Conventions;

namespace NWheels.Entities.Core
{
    public class HiloGeneratorMetadataConvention : IMetadataConvention
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
                    if ( keyProperty.DefaultValueGeneratorType == null && keyProperty.ClrType == typeof(int) && !keyProperty.ContractPropertyInfo.CanWrite )
                    {
                        keyProperty.DefaultValueGeneratorType = typeof(HiloIntegerIdGenerator);
                    }
                }
            }
        }
    }
}
