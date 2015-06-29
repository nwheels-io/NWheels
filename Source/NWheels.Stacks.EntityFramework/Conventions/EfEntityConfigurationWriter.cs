using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.Entities;
using NWheels.DataObjects;
using System.Linq.Expressions;
using Hapil.Members;
using NWheels.Conventions.Core;
using NWheels.Extensions;
using NWheels.Stacks.EntityFramework.Impl;
using TT = Hapil.TypeTemplate;

namespace NWheels.Stacks.EntityFramework.Conventions
{
    internal class EfEntityConfigurationWriter
    {
        private readonly ITypeMetadata _metadata;
        private readonly MethodWriterBase _method;
        private readonly Operand<DbModelBuilder> _model;
        private readonly Operand<ITypeMetadataCache> _metadataCache;
        private readonly Local<ITypeMetadata> _typeMetadataLocal;
        private readonly Local<object> _entityTypeConfigurationLocal;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EfEntityConfigurationWriter(
            ITypeMetadata metadata,
            MethodWriterBase method,
            Operand<DbModelBuilder> model,
            Operand<ITypeMetadataCache> metadataCache,
            Local<ITypeMetadata> typeMetadataLocal,
            Local<object> entityTypeConfigurationLocal)
        {
            _metadataCache = metadataCache;
            _model = model;
            _method = method;
            _metadata = metadata;
            _typeMetadataLocal = typeMetadataLocal;
            _entityTypeConfigurationLocal = entityTypeConfigurationLocal;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void WriteEntityTypeConfiguration()
        {
            var m = _method;

            using ( TT.CreateScope<TT.TImpl>(_metadata.GetImplementationBy<EntityObjectFactory>()) )
            {
                _typeMetadataLocal.Assign(_metadataCache.Func<Type, ITypeMetadata>(x => x.GetTypeMetadata, m.Const(_metadata.ContractType)));
                _entityTypeConfigurationLocal.Assign(Static.Func(EfModelApi.EntityType<TT.TImpl>, _model, _typeMetadataLocal));

                foreach ( var property in _metadata.Properties )
                {
                    //Static.Void(Console.WriteLine, m.Const("CONFIGURING PROPERTY : " + property.ToString()));

                    switch ( property.Kind )
                    {
                        case PropertyKind.Scalar:
                            WriteScalarPropertyConfiguration(property);
                            break;
                        case PropertyKind.Relation:
                            WriteRelationPropertyConfiguration(property);
                            break;
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void WriteScalarPropertyConfiguration(IPropertyMetadata property)
        {
            if ( property.RelationalMapping == null || string.IsNullOrEmpty(property.RelationalMapping.ColumnName) )
            {
                return;
            }

            var m = _method;

            if ( property.ClrType.IsNullableValueType() )
            {
                using ( TT.CreateScope<TT.TStruct>(Nullable.GetUnderlyingType(property.ClrType)) )
                {
                    Static.GenericFunc(
                        (e, p) => EfModelApi.NullableValueTypePrimitiveProperty<TT.TImpl, TT.TStruct>(e, p),
                        _entityTypeConfigurationLocal.CastTo<EntityTypeConfiguration<TT.TImpl>>(),
                        _typeMetadataLocal.Func<string, IPropertyMetadata>(x => x.GetPropertyByName, m.Const(property.Name)));
                }
            }
            else if ( property.ClrType.IsValueType )
            {
                using ( TT.CreateScope<TT.TStruct>(property.ClrType) )
                {
                    Static.GenericFunc(
                        (e, p) => EfModelApi.ValueTypePrimitiveProperty<TT.TImpl, TT.TStruct>(e, p),
                        _entityTypeConfigurationLocal.CastTo<EntityTypeConfiguration<TT.TImpl>>(),
                        _typeMetadataLocal.Func<string, IPropertyMetadata>(x => x.GetPropertyByName, m.Const(property.Name)));
                }
            }
            else if ( property.ClrType == typeof(string) )
            {
                Static.GenericFunc(
                    (e, p) => EfModelApi.StringProperty<TT.TImpl>(e, p),
                    _entityTypeConfigurationLocal.CastTo<EntityTypeConfiguration<TT.TImpl>>(),
                    _typeMetadataLocal.Func<string, IPropertyMetadata>(x => x.GetPropertyByName, m.Const(property.Name)));
            }
            else if ( property.ClrType == typeof(byte[]) )
            {
                Static.GenericFunc(
                    (e, p) => EfModelApi.ByteArrayProperty<TT.TImpl>(e, p),
                    _entityTypeConfigurationLocal.CastTo<EntityTypeConfiguration<TT.TImpl>>(),
                    _typeMetadataLocal.Func<string, IPropertyMetadata>(x => x.GetPropertyByName, m.Const(property.Name)));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void WriteRelationPropertyConfiguration(IPropertyMetadata property)
        {
            if ( property.Relation.Multiplicity == RelationMultiplicity.ManyToOne )
            {
                var m = _method;

                using ( TT.CreateScope<TT.TImpl2>(property.Relation.RelatedPartyType.GetImplementationBy<EntityObjectFactory>()) )
                {
                    Static.GenericFunc(
                        (e, p) => EfModelApi.ManyToOneRelationProperty<TT.TImpl, TT.TImpl2>(e, p),
                        _entityTypeConfigurationLocal.CastTo<EntityTypeConfiguration<TT.TImpl>>(),
                        _typeMetadataLocal.Func<string, IPropertyMetadata>(x => x.GetPropertyByName, m.Const(property.Name)));
                }
            }
        }
    }
}
