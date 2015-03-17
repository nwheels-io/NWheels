using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
using NWheels.Entities;

namespace NWheels.Core.DataObjects
{
    public class TypeMetadataCache : ITypeMetadataCache
    {
        private readonly DataObjectConventions _dataObjectConventions;
        private readonly IRelationalMappingConvention _relationalMappingConvention;
        private readonly ConcurrentDictionary<Type, TypeMetadataBuilder> _metadataByContractType = new ConcurrentDictionary<Type, TypeMetadataBuilder>();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeMetadataCache(DataObjectConventions dataObjectConventions, IRelationalMappingConvention relationalMappingConvention)
        {
            _dataObjectConventions = dataObjectConventions;
            _relationalMappingConvention = relationalMappingConvention;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ITypeMetadata GetTypeMetadata(Type contract)
        {
            return _metadataByContractType.GetOrAdd(contract, BuildTypeMetadata);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void EnsureRelationalMapping(ITypeMetadata type)
        {
            var metadata = (TypeMetadataBuilder)type;
            metadata.EnsureRelationalMapping(_relationalMappingConvention);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal TypeMetadataBuilder FindTypeMetadataAllowIncomplete(Type contract)
        {
            TypeMetadataBuilder metadata;
            var entriesBeingBuilt = _s_entriesBeingBuilt;

            if ( entriesBeingBuilt != null && entriesBeingBuilt.TryGetValue(contract, out metadata) )
            {
                return metadata;
            }

            return (TypeMetadataBuilder)GetTypeMetadata(contract);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private TypeMetadataBuilder BuildTypeMetadata(Type contract)
        {
            var entriesBeingBuilt = _s_entriesBeingBuilt;
            var ownEntriesBeingBuilt = (entriesBeingBuilt == null);

            if ( ownEntriesBeingBuilt )
            {
                entriesBeingBuilt = new Dictionary<Type, TypeMetadataBuilder>();
                _s_entriesBeingBuilt = entriesBeingBuilt;
            }

            try
            {
                var builder = new TypeMetadataBuilder();
                entriesBeingBuilt.Add(contract, builder);

                var constructor = new TypeMetadataBuilderConstructor(new DataObjectConventions());
                constructor.ConstructMetadata(contract, builder, cache: this);

                return builder;
            }
            finally
            {
                if ( ownEntriesBeingBuilt )
                {
                    _s_entriesBeingBuilt = null;
                }
                else
                {
                    entriesBeingBuilt.Remove(contract);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ThreadStatic]
        private static Dictionary<Type, TypeMetadataBuilder> _s_entriesBeingBuilt;
    }
}
