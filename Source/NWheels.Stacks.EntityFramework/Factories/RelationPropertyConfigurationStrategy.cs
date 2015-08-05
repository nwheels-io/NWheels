using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.DataObjects;
using TT = Hapil.TypeTemplate;

namespace NWheels.Stacks.EntityFramework.Factories
{
    public class RelationPropertyConfigurationStrategy : PropertyConfigurationStrategy
    {
        public RelationPropertyConfigurationStrategy(ObjectFactoryContext factoryContext, ITypeMetadata metaType, IPropertyMetadata metaProperty)
            : base(factoryContext, metaType, metaProperty)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of PropertyConfigurationStrategy

        public override void OnWritingEfModelConfiguration(
            MethodWriterBase method, 
            Operand<DbModelBuilder> modelBuilder, 
            Operand<ITypeMetadata> typeMetadata,
            Operand<ITypeMetadataCache> metadataCache, 
            Operand<EntityTypeConfiguration<TypeTemplate.TImpl>> typeConfig)
        {
            var m = method;
            var relatedPartyImplementationType = FindImpementationType(MetaProperty.Relation.RelatedPartyType.ContractType);

            using ( TT.CreateScope<TT.TImpl2>(relatedPartyImplementationType) )
            {
                switch ( MetaProperty.Relation.Multiplicity )
                {
                    case RelationMultiplicity.OneToMany:
                        if ( MetaProperty.Relation.InverseProperty == null )
                        {
                            Static.GenericFunc(
                                (e, p) => EfModelApi.OneToManyNoInverseRelationProperty<TT.TImpl, TT.TImpl2>(e, p),
                                typeConfig,
                                typeMetadata.Func<string, IPropertyMetadata>(x => x.GetPropertyByName, m.Const(MetaProperty.Name)));
                            //EfModelApi.OneToManyNoInverseRelationProperty<EfEntityObject_Customer, EfEntityObject_ContactDetail>(entity, typeMetadata.GetPropertyByName("ContactDetails"));
                        }
                        break;
                    case RelationMultiplicity.ManyToOne:
                        Static.GenericFunc(
                            (e, p) => EfModelApi.ManyToOneRelationProperty<TT.TImpl, TT.TImpl2>(e, p),
                            typeConfig,
                            typeMetadata.Func<string, IPropertyMetadata>(x => x.GetPropertyByName, m.Const(MetaProperty.Name)));
                        break;
                    case RelationMultiplicity.ManyToMany:
                        Static.GenericFunc(
                            (e, p, c) => EfModelApi.ManyToManyRelationProperty<TT.TImpl, TT.TImpl2>(e, p, c),
                            typeConfig,
                            typeMetadata.Func<string, IPropertyMetadata>(x => x.GetPropertyByName, m.Const(MetaProperty.Name)),
                            metadataCache);
                        break;
                }
            }
        }

        #endregion
    }
}
