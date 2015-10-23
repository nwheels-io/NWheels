using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hapil;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.DataObjects.Core.Factories;
using NWheels.Extensions;
using TT = Hapil.TypeTemplate;

namespace NWheels.Stacks.EntityFramework.Factories
{
    public class InverseManyToManyCollectionConvention : ImplementationConvention
    {
        private readonly ObjectFactoryContext _factoryContext;
        private readonly ITypeMetadataCache _metadataCache;
        private readonly ITypeMetadata _metaType;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public InverseManyToManyCollectionConvention(ObjectFactoryContext factoryContext, ITypeMetadataCache metadataCache, ITypeMetadata metaType)
            : base(Will.ImplementBaseClass)
        {
            _factoryContext = factoryContext;
            _metadataCache = metadataCache;
            _metaType = metaType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementBaseClass(ImplementationClassWriter<TypeTemplate.TBase> writer)
        {
            var incomingRelations = _metadataCache.GetIncomingRelations(
                targetType: _metaType, 
                sourcePredicate: p => p.Relation.Multiplicity == RelationMultiplicity.ManyToMany && p.Relation.InverseProperty == null)
                .ToArray();

            foreach ( var relationSource in incomingRelations )
            {
                var inversePropertyName = EfModelApi.GetGeneratedInversePropertyName(relationSource);
                var relationSourceImplementationType = 
                    _factoryContext.Factory.FindDynamicType(relationSource.DeclaringContract.ContractType);

                using ( TT.CreateScope<TT.TConcrete>(relationSourceImplementationType) )
                {
                    writer.NewVirtualWritableProperty<ICollection<TT.TConcrete>>(inversePropertyName).ImplementAutomatic();
                }
            }
        }
    }
}
