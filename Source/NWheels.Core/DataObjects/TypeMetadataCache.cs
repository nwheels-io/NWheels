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
        private readonly Dictionary<Type, MixinRegistration[]> _mixinsByPrimaryContract;
        private readonly ConcurrentDictionary<Type, TypeMetadataBuilder> _metadataByContractType = new ConcurrentDictionary<Type, TypeMetadataBuilder>();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeMetadataCache(
            DataObjectConventions dataObjectConventions,
            IRelationalMappingConvention relationalMappingConvention,
            IEnumerable<MixinRegistration> mixinRegistrations)
        {
            _dataObjectConventions = dataObjectConventions;
            _relationalMappingConvention = relationalMappingConvention;
            _mixinsByPrimaryContract = mixinRegistrations.GroupBy(r => r.TargetContract).ToDictionary(g => g.Key, g => g.ToArray());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeMetadataCache(
            DataObjectConventions dataObjectConventions, 
            IRelationalMappingConvention relationalMappingConvention)
            : this(dataObjectConventions, relationalMappingConvention, mixinRegistrations: new MixinRegistration[0])
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ITypeMetadata GetTypeMetadata(Type primaryContract)
        {
            return _metadataByContractType.GetOrAdd(primaryContract, BuildTypeMetadata);
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

        private TypeMetadataBuilder BuildTypeMetadata(Type primaryContract)
        {
            MixinRegistration[] mixinRegistrations;
            Type[] mixinContracts;

            if ( _mixinsByPrimaryContract.TryGetValue(primaryContract, out mixinRegistrations) )
            {
                mixinContracts = mixinRegistrations.Select(r => r.MixinContract).ToArray();
            }
            else
            {
                mixinContracts = Type.EmptyTypes;
            }

            return BuildTypeMetadata(primaryContract, mixinContracts);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private TypeMetadataBuilder BuildTypeMetadata(Type primaryContract, Type[] mixinContracts)
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
                entriesBeingBuilt.Add(primaryContract, builder);

                var constructor = new TypeMetadataBuilderConstructor(new DataObjectConventions());
                constructor.ConstructMetadata(primaryContract, mixinContracts, builder, cache: this);

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
                    entriesBeingBuilt.Remove(primaryContract);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ThreadStatic]
        private static Dictionary<Type, TypeMetadataBuilder> _s_entriesBeingBuilt;
    }
}
