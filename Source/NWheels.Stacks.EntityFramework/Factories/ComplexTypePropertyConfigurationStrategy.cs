using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.DataObjects.Core.Factories;
using NWheels.Extensions;
using TT = Hapil.TypeTemplate;

namespace NWheels.Stacks.EntityFramework.Factories
{
    public class ComplexTypePropertyConfigurationStrategy : PropertyConfigurationStrategy
    {
        public ComplexTypePropertyConfigurationStrategy(
            ObjectFactoryContext factoryContext, 
            ITypeMetadata metaType, 
            IPropertyMetadata metaProperty)
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
            var entityMetaPropertyLocal = m.Local<IPropertyMetadata>();
            var complexMetaPropertyLocal = m.Local<IPropertyMetadata>();

            Type complexIimplementationType = FindImpementationType(MetaProperty.ClrType);

            using ( TT.CreateScope<TT.TImpl2>(complexIimplementationType) )
            {
                entityMetaPropertyLocal.Assign(typeMetadata.Func<string, IPropertyMetadata>(x => x.GetPropertyByName, m.Const(MetaProperty.Name)));
                modelBuilder.Func<ComplexTypeConfiguration<TT.TImpl2>>(x => x.ComplexType<TT.TImpl2>);

                foreach ( var complexTypeProperty in MetaProperty.Relation.RelatedPartyType.Properties )
                {
                    using ( TT.CreateScope<TT.TProperty>(complexTypeProperty.StorageClrType()) )
                    {
                        complexMetaPropertyLocal.Assign(
                            entityMetaPropertyLocal
                            .Prop(x => x.Relation)
                            .Prop(x => x.RelatedPartyType)
                            .Func<string, IPropertyMetadata>(x => x.GetPropertyByName, m.Const(complexTypeProperty.Name)));

                        Static.Void(EfModelApi.ComplexTypeProperty<TT.TImpl, TT.TProperty>, 
                            modelBuilder,
                            typeConfig,
                            entityMetaPropertyLocal,
                            complexMetaPropertyLocal);
                    }
                }
            }
        }

        #endregion
    }
}
