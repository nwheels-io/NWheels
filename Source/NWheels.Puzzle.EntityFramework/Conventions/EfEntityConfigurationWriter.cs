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
using NWheels.Puzzle.EntityFramework.Impl;
using TT = Hapil.TypeTemplate;

namespace NWheels.Puzzle.EntityFramework.Conventions
{
    internal class EfEntityConfigurationWriter
    {
        private readonly ITypeMetadata _metadata;
        private readonly MethodWriterBase _method;
        private readonly Operand<DbModelBuilder> _model;
        private readonly Local<ParameterExpression> _parameterExpressionLocal;
        private readonly MethodInfo _nonNullablePrimitivePropertyConfigMethod;
        private readonly MethodInfo _nullablePrimitivePropertyConfigMethod;
        private readonly MethodInfo _hasRequiredPropertyConfigMethod;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EfEntityConfigurationWriter(
            ITypeMetadata metadata,
            MethodWriterBase method,
            Operand<DbModelBuilder> model,
            Local<ParameterExpression> parameterExpressionLocal)
        {
            _model = model;
            _method = method;
            _metadata = metadata;
            _parameterExpressionLocal = parameterExpressionLocal;

            FindtTypeConfigurationMethods(
                _metadata.ImplementationType,
                out _nonNullablePrimitivePropertyConfigMethod,
                out _nullablePrimitivePropertyConfigMethod,
                out _hasRequiredPropertyConfigMethod);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void WriteEntityTypeConfiguration()
        {
            var m = _method;

            using ( TT.CreateScope<TT.TImpl>(_metadata.ImplementationType) )
            {
                var implementationTypeLocal = m.Local<Type>();
                implementationTypeLocal.Assign(m.Const(TT.Resolve<TT.TImpl>()));

                //BUG in Hapil? when replacing implementationTypeLocal with m.Const(TT.Resolve<TT.TImpl>()), Reflector fails to desassemble IL into C#
                //_parameterExpressionLocal.Assign(Static.Func(Expression.Parameter, implementationTypeLocal, m.Const("e")));

                var typeConfig = _method.Local(initialValue: _model.Func<EntityTypeConfiguration<TT.TImpl>>(x => x.Entity<TT.TImpl>));

                typeConfig.Func<string, EntityTypeConfiguration<TT.TImpl>>(x => x.ToTable, m.Const(_metadata.RelationalMapping.PrimaryTableName));
                typeConfig.Func<string, EntityTypeConfiguration<TT.TImpl>>(x => x.HasEntitySetName, m.Const(_metadata.Name));

                foreach ( var property in _metadata.Properties )
                {
                    Static.Void(Console.WriteLine, m.Const("CONFIGURING PROPERTY : " + property.ToString()));

                    switch ( property.Kind )
                    {
                        case PropertyKind.Scalar:
                            WriteScalarPropertyConfiguration(property, typeConfig);
                            break;
                        case PropertyKind.Relation:
                            WriteRelationPropertyConfiguration(property, typeConfig);
                            break;
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void WriteScalarPropertyConfiguration(IPropertyMetadata property, Local<EntityTypeConfiguration<TT.TImpl>> typeConfig)
        {
            var m = _method;

            if ( property.RelationalMapping != null && !string.IsNullOrEmpty(property.RelationalMapping.ColumnName) )
            {
                var propertyConfig = m.Local<PrimitivePropertyConfiguration>();
                propertyConfig.Assign(WriteGetPrimitivePropertyConfiguration(property, typeConfig));
                propertyConfig.Func<string, PrimitivePropertyConfiguration>(x => x.HasColumnName, m.Const(property.RelationalMapping.ColumnName));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void WriteRelationPropertyConfiguration(IPropertyMetadata property, Operand<EntityTypeConfiguration<TT.TImpl>> typeConfig)
        {
            if ( property.Relation.RelationKind == RelationKind.ManyToOne && property.Relation.ThisPartyKind == RelationPartyKind.Dependent )
            {
                WriteManyToOnePropertyConfiguration(property, typeConfig);
            }
            else if ( property.Relation.RelationKind == RelationKind.OneToMany && property.Relation.ThisPartyKind == RelationPartyKind.Principal )
            {
                WriteOneToManyPropertyConfiguration(property, typeConfig);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void WriteOneToManyPropertyConfiguration(IPropertyMetadata property, Operand<EntityTypeConfiguration<TT.TImpl>> typeConfig)
        {
            //var m = _method;

            //using ( TT.CreateScope<TT.TImpl, TT.TImpl2>(_metadata.ImplementationType, property.Relation.RelatedPartyType.ImplementationType) )
            //{
            //    typeConfig
            //        .Func<Expression<Func<TT.TImpl, TT.TImpl2>>, RequiredNavigationPropertyConfiguration<TT.TImpl, TT.TImpl2>>(
            //            _hasRequiredPropertyConfigMethod.MakeGenericMethod(property.Relation.RelatedPartyType.ImplementationType),
            //                WriteNewPropertyExpression<TT.TImpl2>(property).CastTo<Expression<Func<TT.TImpl, TT.TImpl2>>>())
            //        .Func<ForeignKeyNavigationPropertyConfiguration>(
            //            x => x.WithRequiredDependent)
            //        .Func<Action<ForeignKeyAssociationMappingConfiguration>, CascadableNavigationPropertyConfiguration>(
            //            x => x.Map,
            //                m.Delegate<ForeignKeyAssociationMappingConfiguration>((mm, map) =>
            //                {
            //                    map.Func<string[], ForeignKeyAssociationMappingConfiguration>(
            //                        x => x.MapKey,
            //                            mm.NewArray<string>(property.RelationalMapping.ColumnName));
            //                })
            //        );
            //}
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void WriteManyToOnePropertyConfiguration(IPropertyMetadata property, Operand<EntityTypeConfiguration<TT.TImpl>> typeConfig)
        {
            var m = _method;

            using ( TT.CreateScope<TT.TImpl, TT.TImpl2>(_metadata.ImplementationType, property.Relation.RelatedPartyType.ImplementationType) )
            {
                if ( property.Relation.InverseProperty != null && property.Relation.InverseProperty.ClrType.IsCollectionType() )
                {
                    typeConfig
                        .Func<Expression<Func<TT.TImpl, TT.TImpl2>>, RequiredNavigationPropertyConfiguration<TT.TImpl, TT.TImpl2>>(
                            _hasRequiredPropertyConfigMethod.MakeGenericMethod(property.Relation.RelatedPartyType.ImplementationType),
                                WriteNewPropertyExpression<TT.TImpl, TT.TImpl2>(property).CastTo<Expression<Func<TT.TImpl, TT.TImpl2>>>())
                        .Func<Expression<Func<TT.TImpl2, ICollection<TT.TImpl>>>, ForeignKeyNavigationPropertyConfiguration>(
                            x => x.WithMany,
                                WriteNewPropertyExpression<TT.TImpl2, ICollection<TT.TImpl>>(property.Relation.InverseProperty).CastTo<Expression<Func<TT.TImpl2, ICollection<TT.TImpl>>>>())
                        .Func<Action<ForeignKeyAssociationMappingConfiguration>, CascadableNavigationPropertyConfiguration>(
                            x => x.Map,
                                m.Delegate<ForeignKeyAssociationMappingConfiguration>((mm, map) => {
                                    map.Func<string[], ForeignKeyAssociationMappingConfiguration>(
                                        x => x.MapKey,
                                            mm.NewArray<string>(property.RelationalMapping.ColumnName));
                                })
                        );
                }
                else
                {
                    typeConfig
                        .Func<Expression<Func<TT.TImpl, TT.TImpl2>>, RequiredNavigationPropertyConfiguration<TT.TImpl, TT.TImpl2>>(
                            _hasRequiredPropertyConfigMethod.MakeGenericMethod(property.Relation.RelatedPartyType.ImplementationType),
                                WriteNewPropertyExpression<TT.TImpl, TT.TImpl2>(property).CastTo<Expression<Func<TT.TImpl, TT.TImpl2>>>())
                        .Func<ForeignKeyNavigationPropertyConfiguration>(
                            x => x.WithRequiredDependent)
                        .Func<Action<ForeignKeyAssociationMappingConfiguration>, CascadableNavigationPropertyConfiguration>(
                            x => x.Map,
                                m.Delegate<ForeignKeyAssociationMappingConfiguration>((mm, map) => {
                                    map.Func<string[], ForeignKeyAssociationMappingConfiguration>(
                                        x => x.MapKey,
                                            mm.NewArray<string>(property.RelationalMapping.ColumnName));
                                })
                        );
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Operand<PrimitivePropertyConfiguration> WriteGetPrimitivePropertyConfiguration(
            IPropertyMetadata property, 
            Operand<EntityTypeConfiguration<TT.TImpl>> typeConfig)
        {
            var m = _method;

            if ( property.ClrType.IsNullableValueType() )
            {
                using ( TT.CreateScope<TT.TStruct>(Nullable.GetUnderlyingType(property.ClrType)) )
                {
                    return typeConfig.Func<Expression<Func<TT.TImpl, TT.TStruct?>>, PrimitivePropertyConfiguration>(
                        _nullablePrimitivePropertyConfigMethod.MakeGenericMethod(Nullable.GetUnderlyingType(property.ClrType)),
                        WriteNewPropertyExpression<TT.TImpl, TT.TStruct?>(property).CastTo<Expression<Func<TT.TImpl, TT.TStruct?>>>());
                }
            }
            else
            {
                using ( TT.CreateScope<TT.TStruct>(property.ClrType) )
                {
                    if ( property.ClrType.IsValueType )
                    {
                        return typeConfig.Func<Expression<Func<TT.TImpl, TT.TStruct>>, PrimitivePropertyConfiguration>(
                            _nonNullablePrimitivePropertyConfigMethod.MakeGenericMethod(property.ClrType),
                            WriteNewPropertyExpression<TT.TImpl, TT.TStruct>(property).CastTo<Expression<Func<TT.TImpl, TT.TStruct>>>());
                    }
                    else if ( property.ClrType == typeof(string) )
                    {
                        return typeConfig.Func<Expression<Func<TT.TImpl, string>>, PrimitivePropertyConfiguration>(
                            x => x.Property,
                            WriteNewPropertyExpression<TT.TImpl, string>(property).CastTo<Expression<Func<TT.TImpl, string>>>());
                    }
                    else if ( property.ClrType == typeof(byte[]) )
                    {
                        return typeConfig.Func<Expression<Func<TT.TImpl, byte[]>>, PrimitivePropertyConfiguration>(
                            x => x.Property,
                            WriteNewPropertyExpression<TT.TImpl, byte[]>(property).CastTo<Expression<Func<TT.TImpl, byte[]>>>());
                    }
                }
            }

            throw new NotSupportedException(string.Format(
                "Primitive property type '{0}' is not supported by EfEntityConfigurationWriter", 
                property.ClrType.FullName));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IOperand<LambdaExpression> WriteNewPropertyExpression<TObject, TProp>(IPropertyMetadata property)
        {
            var m = _method;

            using ( TT.CreateScope<TT.TImpl, TT.TProperty>(TT.Resolve<TObject>(), TT.Resolve(typeof(TProp))) )
            {
                var parameterExpressionLocal = m.Local<ParameterExpression>(initialValue: Static.Func(Expression.Parameter, m.Const(TT.Resolve<TObject>()), m.Const("e")));

                var propertyGetter = m.Local<MethodInfo>();
                propertyGetter.Assign(m.Const(property.ImplementationPropertyInfo.GetMethod ?? property.ImplementationPropertyInfo.SetMethod));

                return Static.Func<Expression, ParameterExpression[], Expression<Func<TT.TImpl, TT.TProperty>>>(
                    Expression.Lambda<Func<TT.TImpl, TT.TProperty>>,
                    Static.Func<Expression, MethodInfo, MemberExpression>(Expression.Property, parameterExpressionLocal, propertyGetter),
                    m.NewArray<ParameterExpression>(values: parameterExpressionLocal));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void FindtTypeConfigurationMethods(
            Type entityType, 
            out MethodInfo nonNullableProperty, 
            out MethodInfo nullableProperty,
            out MethodInfo hasRequired)
        {
            var allMethods = typeof(EntityTypeConfiguration<>).MakeGenericType(entityType).GetMethods();

            var propertyMethods = allMethods
                .Where(m => m.Name == "Property" && m.IsGenericMethod)
                .ToArray();

            nonNullableProperty = propertyMethods.Single(m => !m.GetParameters()[0].ParameterType.ToString().Contains("System.Nullable"));
            nullableProperty = propertyMethods.Single(m => m.GetParameters()[0].ParameterType.ToString().Contains("System.Nullable"));

            hasRequired = allMethods.Single(p => p.Name == "HasRequired");
        }
    }
}
