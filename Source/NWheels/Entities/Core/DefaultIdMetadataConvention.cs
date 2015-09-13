using System;
using System.Linq;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.Conventions;

namespace NWheels.Entities.Core
{
    public class DefaultIdMetadataConvention : IMetadataConvention
    {
        private readonly Type _entityPartIdMixinType;
        private TypeMetadataCache _metadataCache;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DefaultIdMetadataConvention(Type idPropertyType)
        {
            _entityPartIdMixinType = typeof(IEntityPartId<>).MakeGenericType(idPropertyType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void InjectCache(TypeMetadataCache cache)
        {
            _metadataCache = cache;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Preview(TypeMetadataBuilder type)
        {
            if ( !EntityContractAttribute.IsEntityContract(type.ContractType) )
            {
                return;
            }

            for ( var currentType = type ; currentType != null ; currentType = currentType.BaseType )
            {
                if ( TypeHasPrimaryKey(currentType) )
                {
                    return;
                }
            }

            type.MixinContractTypes.Add(_entityPartIdMixinType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Apply(TypeMetadataBuilder type)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Finalize(TypeMetadataBuilder type)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool IsEntityPartIdMixinType(Type type)
        {
            return (
                type.IsConstructedGenericType && 
                type.GetGenericTypeDefinition() == typeof(IEntityPartId<>));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool TypeHasPrimaryKey(TypeMetadataBuilder type)
        {
            return (
                type.PrimaryKey != null ||
                type.Properties.Any(p => p.Role == PropertyRole.Key) ||
                type.MixinContractTypes.Any(IsEntityPartIdMixinType));
        }
    }
}
