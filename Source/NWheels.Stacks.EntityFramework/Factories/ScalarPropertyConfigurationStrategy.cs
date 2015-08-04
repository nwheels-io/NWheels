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
using NWheels.Extensions;
using TT = Hapil.TypeTemplate;

namespace NWheels.Stacks.EntityFramework.Factories
{
    public class ScalarPropertyConfigurationStrategy : PropertyConfigurationStrategy
    {
        public ScalarPropertyConfigurationStrategy(ObjectFactoryContext factoryContext, ITypeMetadata metaType, IPropertyMetadata metaProperty)
            : base(factoryContext, metaType, metaProperty)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of PropertyConfigurationStrategy

        public override void OnWritingEfModelConfiguration(
            MethodWriterBase method, 
            Operand<DbModelBuilder> modelBuilder, 
            Operand<ITypeMetadata> typeMetadata, 
            Operand<EntityTypeConfiguration<TypeTemplate.TImpl>> typeConfig)
        {
            if ( MetaProperty.RelationalMapping == null || string.IsNullOrEmpty(MetaProperty.RelationalMapping.ColumnName) )
            {
                return;
            }

            var m = method;
            var storageClrType = MetaProperty.StorageClrType();

            if ( storageClrType.IsNullableValueType() )
            {
                using ( TT.CreateScope<TT.TStruct>(Nullable.GetUnderlyingType(MetaProperty.ClrType)) )
                {
                    Static.GenericFunc(
                        (e, p) => EfModelApi.NullableValueTypePrimitiveProperty<TT.TImpl, TT.TStruct>(e, p),
                        typeConfig.CastTo<EntityTypeConfiguration<TT.TImpl>>(),
                        typeMetadata.Func<string, IPropertyMetadata>(x => x.GetPropertyByName, m.Const(MetaProperty.Name)));
                }
            }
            else if ( storageClrType.IsValueType )
            {
                using ( TT.CreateScope<TT.TStruct>(MetaProperty.ClrType) )
                {
                    Static.GenericFunc(
                        (e, p) => EfModelApi.ValueTypePrimitiveProperty<TT.TImpl, TT.TStruct>(e, p),
                        typeConfig.CastTo<EntityTypeConfiguration<TT.TImpl>>(),
                        typeMetadata.Func<string, IPropertyMetadata>(x => x.GetPropertyByName, m.Const(MetaProperty.Name)));
                }
            }
            else if ( storageClrType == typeof(string) )
            {
                Static.GenericFunc(
                    (e, p) => EfModelApi.StringProperty<TT.TImpl>(e, p),
                    typeConfig.CastTo<EntityTypeConfiguration<TT.TImpl>>(),
                    typeMetadata.Func<string, IPropertyMetadata>(x => x.GetPropertyByName, m.Const(MetaProperty.Name)));
            }
            else if ( storageClrType == typeof(byte[]) )
            {
                Static.GenericFunc(
                    (e, p) => EfModelApi.ByteArrayProperty<TT.TImpl>(e, p),
                    typeConfig.CastTo<EntityTypeConfiguration<TT.TImpl>>(),
                    typeMetadata.Func<string, IPropertyMetadata>(x => x.GetPropertyByName, m.Const(MetaProperty.Name)));
            }
        }

        #endregion
    }
}
